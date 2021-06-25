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

        public ProjectsController(ApplicationDbContext context,
                                  IBTProjectService projectService,
                                  IBTCompanyInfoService infoService,
                                  UserManager<BTUser> userManager)
        {
            _context = context;
            _projectService = projectService;
            _infoService = infoService;
            _userManager = userManager;
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

        //GET: Assign Project Manager
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignPM(int id)
        {
            ProjectManagerViewModel model = new();

            //Get company id
            int companyId = User.Identity.GetCompanyId().Value;

            //Get the project
            Project project = new();
            try
            {
                List<Project> projects = await _projectService.GetAllProjectsByCompany(companyId);
                project = projects.FirstOrDefault(p => p.Id == id);
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error getting projects - {ex.Message}");
                throw;
            }
            //Populate VM Project
            model.Project = project;

            //Multi-Select

            //Get users = pm
            List<BTUser> users = new();

            try
            {
                List<BTUser> pm = await _infoService.GetMembersInRoleAsync(Roles.ProjectManager.ToString(), companyId);
                users = pm;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error getting users not on project - {ex.Message}");
                throw;
            }

            //GET all members from the project
            List<string> members = new();

            try
            {
                if(project?.Members != null)
                {
                    members = project.Members.Select(m => m.Id).ToList();
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error getting members on project - {ex.Message}");
                throw;
            }

            // Add users to select in VM
            BTUser currentPM = await _projectService.GetProjectManagerAsync(project.Id);
            model.Users = new SelectList(users, "Id", "FullName", currentPM?.Id);
            model.SelectedUser = currentPM?.Id;

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AssignPM(ProjectManagerViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.SelectedUser != null)
                {
                    string selectedPM = model.SelectedUser;

                    await _projectService.RemoveProjectManagerAsync(model.Project.Id);
                    await _projectService.AddProjectManagerAsync(selectedPM, model.Project.Id);

                    return RedirectToAction("Details", "Projects", new { id = model.Project.Id });
                }
                else
                {
                    // send an error message back
                    //Use a sweet alert or something that works in the template and return it in the view.
                }
            }
            return View(model);
        }

        // GET: Projects/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");

            ViewData["ProjectPriorityId"] = new SelectList(_context.Set<ProjectPriority>(), "Id", "Name");

            var companyId = User.Identity.GetCompanyId().Value;

            var projects = await _projectService.GetAllProjectsByCompany(companyId);

            var project = projects.FirstOrDefault(m => m.Id == id);


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

            return View(project);
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
                return RedirectToAction(nameof(Index));
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
                    // send an error message back
                    //Use a sweet alert or something that works in the template and return it in the view.
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
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Project.Any(e => e.Id == id);
        }
    }
}
