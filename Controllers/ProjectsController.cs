using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SlickTicket.Data;
using SlickTicket.Extensions;
using SlickTicket.Models;
using SlickTicket.Models.Enums;
using SlickTicket.Models.ViewModels;
using SlickTicket.Services.Interfaces;

namespace SlickTicket.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyInfoService _infoService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTNotificationService _notificationService;

        public ProjectsController(ApplicationDbContext context,
                                  IBTProjectService projectService,
                                  IBTCompanyInfoService infoService,
                                  UserManager<BTUser> userManager,
                                  IBTNotificationService notificationService)
        {
            _context = context;
            _projectService = projectService;
            _infoService = infoService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Project.Include(p => p.Company).Include(p => p.ProjectPriority);
            return View(await applicationDbContext.ToListAsync());
        }

        //Company Projects
        [HttpGet]
        [Authorize(Roles = "Administrator, ProjectManager, Developer")]
        public async Task<IActionResult> CompanyProjects(int? id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            var model = await _projectService.GetAllProjectsByCompany(companyId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        //My Projects
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyProjects()
        {
            var myId = (await _userManager.GetUserAsync(User)).Id;
            var model = await _projectService.ListUserProjectsAsync(myId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public async Task<ActionResult> ArchiveToggle(int id)
        {
            var companyId = User.Identity.GetCompanyId().Value;
            Project project = await _context.Project.FindAsync(id);

            if(project.Archived == false)
            {
                project.Archived = true;
            }
            else
            {
                project.Archived = false;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = project.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddManager(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;
            ProjectManagerViewModel model = new();
            var project = (await _projectService.GetAllProjectsByCompanyAsync(companyId))
                                               .FirstOrDefault(p => p.Id == id);

            model.Project.Id = id;
            List<BTUser> users = await _infoService.GetMembersInRoleAsync("ProjectManager", companyId);
            List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Managers = new SelectList(users, "Id", "FullName", (await _projectService.GetProjectManagerAsync(project.Id))?.Id);
            return View(model);
        }


        //POST: AddManager
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddManager(int id, [Bind("ProjectId,NewManagerId,Managers")] ProjectManagerViewModel model)
        {
            if (User.IsInRole("DemoUser"))
            {
                return RedirectToAction("DemoError", "Home");
            }
            if (ModelState.IsValid)
            {
                Project project = await _context.Project.FindAsync(id);
                model.Project = project;

                var oldManager = await _projectService.GetProjectManagerAsync(id);
                await _projectService.RemoveProjectManagerAsync(model.Project.Id);
                await _projectService.AddProjectManagerAsync(model.NewManagerId, model.Project.Id);
                if (model.NewManagerId != null && model.NewManagerId != oldManager?.Id)
                {
                    var senderId = _userManager.GetUserId(User);
                    //for each selected user not in original list
                    Notification notification = new()
                    {
                        Created = DateTime.Now,
                        Message = $"You are now managing project: {project.Name}",
                        SenderId = senderId,
                        RecipientId = model.NewManagerId
                    };
                    await _notificationService.SaveNotificationAsync(notification);
                    if (oldManager.Id != _userManager.GetUserId(User))
                    {

                        notification = new()
                        {
                            Created = DateTime.Now,
                            Message = $"You are no longer managing project: {project.Name}",
                            SenderId = senderId,
                            RecipientId = oldManager.Id
                        };
                        await _notificationService.SaveNotificationAsync(notification);
                    }
                    await _projectService.AddUserToProjectAsync(model.NewManagerId, model.Project.Id);

                }
                else
                {
                    //send an error message to user, that they didnt'  select anyone
                }
                return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
            }
            return View(model);
        }

        // GET: Projects/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");

            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name");

            int companyId = User.Identity.GetCompanyId().Value;
            ProjectManagerViewModel model = new();
            var project = (await _projectService.GetAllProjectsByCompanyAsync(companyId))
                                               .FirstOrDefault(p => p.Id == id);

            model.Project.Id = (int)id;
            model.Project = project;
            List<BTUser> users = await _infoService.GetMembersInRoleAsync("ProjectManager", companyId);
            List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Managers = new SelectList(users, "Id", "FullName", (await _projectService.GetProjectManagerAsync(project.Id))?.Id);
            var pm = await _projectService.GetProjectManagerAsync(project.Id);

            if (project.Archived == true)
            {
                ViewData["Archived"] = "Yes";
            }

            ViewData["Archived"] = "No";

            if (project == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Administrator, ProjectManager")]
        public IActionResult Create()
        {

            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFileName,ImageContentType")] Project project)
        {

            int companyId = User.Identity.GetCompanyId().Value;

            if (ModelState.IsValid)
            {
                project.CompanyId = companyId;

                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction("CompanyProjects");
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        [Authorize(Roles = "Administrator, ProjectManager")]
        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Id", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFileName,ImageContentType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Company, "Id", "Id", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Id", project.ProjectPriorityId);
            return View(project);
        }



        [HttpGet]
        [Authorize(Roles = "Administrator, ProjectManager")]
        public async Task<IActionResult> AssignUsers(int id)
        {
            ProjectMembersViewModel model = new();

            //get companyId
            int companyId = User.Identity.GetCompanyId().Value;

            Project project = (await _projectService.GetAllProjectsByCompany(companyId))
                                     .FirstOrDefault(p => p.Id == id);

            model.Project = project;
            List<BTUser> developers = await _infoService.GetMembersInRoleAsync(Roles.Developer.ToString(), companyId);
            List<BTUser> submitters = await _infoService.GetMembersInRoleAsync(Roles.Submitter.ToString(), companyId);
            List<BTUser> users = developers.Concat(submitters).ToList();
            List<string> members = project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(users, "Id", "FullName", members);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, ProjectManager")]
        public async Task<IActionResult> AssignUsers(ProjectMembersViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUsers != null)
                {
                    List<string> memberIds = (await _projectService.GetMembersWithoutPMAsync(model.Project.Id))
                                                                    .Select(m => m.Id).ToList();

                    foreach (string id in memberIds)
                    {
                        await _projectService.RemoveUserFromProjectAsync(id, model.Project.Id);
                    }
                    foreach (string id in model.SelectedUsers)
                    {
                        await _projectService.AddUserToProjectAsync(id, model.Project.Id);
                    }
                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
                else
                {

                }
            }
            return View(model);
        }



        // GET: Projects/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Project.FindAsync(id);
            _context.Project.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("CompanyProjects");
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}
