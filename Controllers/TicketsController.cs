using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SlickTicket.Data;
using SlickTicket.Extensions;
using SlickTicket.Models;
using SlickTicket.Services;
using SlickTicket.Services.Interfaces;
using SlickTicket.Models.ViewModels;
using System.IO;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;

namespace SlickTicket.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTTicketService _ticketService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTHistoryService _historyService;
        private readonly IBTCompanyInfoService _infoService;
        private readonly IBTNotificationService _notificationService;
        private readonly SearchService _searchService;

        public TicketsController(ApplicationDbContext context, 
                                 IBTTicketService ticketService, 
                                 UserManager<BTUser> userManager, 
                                 IBTProjectService projectService, 
                                 IBTHistoryService historyService,
                                 IBTCompanyInfoService infoService,
                                 IBTNotificationService notificationService,
                                 SearchService searchService)
        {
            _context = context;
            _ticketService = ticketService;
            _userManager = userManager;
            _projectService = projectService;
            _historyService = historyService;
            _infoService = infoService;
            _notificationService = notificationService;
            _searchService = searchService;
        }

        // GET: Tickets
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ticket
                                         //Include = eager loading
                                        .Include(t => t.DeveloperUser)
                                        .Include(t => t.OwnerUser)
                                        .Include(t => t.Project)
                                        .Include(t => t.TicketPriority)
                                        .Include(t => t.TicketStatus)
                                        .Include(t => t.TicketType);

            return View(await applicationDbContext.ToListAsync());
        }


        // GET: Tickets/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var companyId = User.Identity.GetCompanyId().Value;

            Ticket model = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == id);

            ViewData["CurrentUserId"] = _userManager.GetUserId(User);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _context.TicketAttachment.FindAsync(id);
            string fileName = ticketAttachment.FileName;
            byte[] fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            return File(fileData, $"application/{ext}");

        }
        [Authorize(Roles = "Administrator, ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignTicket(int? ticketId)
        {
            if (!ticketId.HasValue)
            {
                return RedirectToAction("CompanyTickets");
            }

            AssignDeveloperViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;

            model.Ticket = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t=>t.Id==ticketId);
            model.Developers = new SelectList(await _projectService.DevelopersOnProjectAsync(model.Ticket.ProjectId), "Id", "FullName");

            return View(model);
        }

        [Authorize(Roles = "Administrator, ProjectManager")]
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AssignTicket(AssignDeveloperViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.DeveloperId))
            {
                int companyId = User.Identity.GetCompanyId().Value;

                BTUser user = await _userManager.GetUserAsync(User);
                BTUser developer = (await _infoService.GetAllMembersAsync(companyId)).FirstOrDefault(m => m.Id == viewModel.DeveloperId);
                BTUser projectManager = await _projectService.GetProjectManagerAsync(viewModel.Ticket.ProjectId);

                Ticket oldTicket = await _context.Ticket.Include(t => t.TicketPriority)
                                        .Include(t => t.TicketStatus)
                                        .Include(t => t.TicketType)
                                        .Include(t => t.Project)
                                        .Include(t => t.DeveloperUser)
                                        .AsNoTracking().FirstOrDefaultAsync(t => t.Id == viewModel.Ticket.Id);

                await _ticketService.AssignTicketAsync(viewModel.Ticket.Id, viewModel.DeveloperId);

                Ticket newTicket = await _context.Ticket.Include(t => t.TicketPriority)
                                        .Include(t => t.TicketStatus)
                                        .Include(t => t.TicketType)
                                        .Include(t => t.Project)
                                        .Include(t => t.DeveloperUser)
                                        .AsNoTracking().FirstOrDefaultAsync(t => t.Id == viewModel.Ticket.Id);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, user.Id);
            }
            return RedirectToAction("Details", new { id = viewModel.Ticket.Id });
        }

        [Authorize(Roles = "Administrator, ProjectManager")]
        public async Task<IActionResult> ArchiveToggle(int id)
        {
            var companyId = User.Identity.GetCompanyId().Value;
            Ticket ticket = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).FirstOrDefault(t => t.Id == id);

            if (ticket.Archived == false)
            {
                ticket.Archived = true;
            }
            else
            {
                ticket.Archived = false;
            }
            await _context.SaveChangesAsync();
            if (ticket.Archived == true)
            {
                TempData["StatusMessage"] = $"{ticket.Title} was successfully archived";
            }
            else
            {
                TempData["StatusMessage"] = $"{ticket.Title} was successfully re-opened";
            }
                
            return RedirectToAction("Details",  new { id = ticket.Id });

        }

        [HttpGet]
        [Authorize]
        public IActionResult SearchIndex(List<Ticket> tickets)
        {
            return View(tickets);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchIndex(string searchString)
        {
            //I need a set of results stemming from this search string
            var tickets = await _searchService.SearchActiveContent(searchString);

            return View("SearchIndex", tickets);

        }

        // GET: Tickets/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            //get current user
            BTUser btUser = await _userManager.GetUserAsync(User);

            //get current user's company Id
            int companyId = User.Identity.GetCompanyId().Value;

            //TODO: Filter List
            if (User.IsInRole("Administrator"))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.ListUserProjectsAsync(btUser.Id), "Id", "Name");
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name");



            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name");

            return View();
        }


        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                BTUser user = await _userManager.GetUserAsync(User);

                ticket.Created = DateTimeOffset.Now;

                string userId = _userManager.GetUserId(User);
                ticket.OwnerUserId = userId;
                ticket.TicketStatusId = (await _ticketService.LookupTicketStatusIdAsync("New")).Value;

                await _context.AddAsync(ticket);
                await _context.SaveChangesAsync();

                #region Add History
                //Add history
                Ticket newTicket = await _context.Ticket
                                                 .Include(t => t.TicketPriority)
                                                 .Include(t => t.TicketStatus)
                                                 .Include(t => t.TicketType)
                                                 .Include(t => t.Project)
                                                 .Include(t => t.DeveloperUser)
                                                 .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                await _historyService.AddHistoryAsync(null, newTicket, user.Id);
                #endregion

                #region Notification
                BTUser projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);
                int companyId = User.Identity.GetCompanyId().Value;
                
                Notification notification = new()
                {
                    TicketId = ticket.Id,
                    Title = "New Ticket",
                    Message = $"New Ticket: {ticket?.Title}, was created by {user?.FullName}",
                    Created = DateTimeOffset.Now,
                    SenderId = user?.Id,
                    RecipientId = projectManager?.Id
                };

                if (projectManager != null)
                {
                    await _notificationService.SaveNotificationAsync(notification);
                }
                else
                {
                    //Admin notification
                    await _notificationService.AdminsNotificationAsync(notification, companyId);
                }
                #endregion

                return RedirectToAction("Details","Projects", new { id = ticket.ProjectId});
            }

            return View(ticket);
        }

        [Authorize]
        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId().Value;

            ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompany(companyId), "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }


        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedDate,ProjectId,StatusId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }
            Notification notification;

            if (ModelState.IsValid)
            {

                int companyId = User.Identity.GetCompanyId().Value;
                BTUser user = await _userManager.GetUserAsync(User);
                BTUser pm = await _projectService.GetProjectManagerAsync(ticket.ProjectId);

                Ticket oldTicket = await _context.Ticket.Include(t => t.TicketPriority)
                                                        .Include(t => t.TicketStatus)
                                                        .Include(t => t.TicketType)
                                                        .Include(t => t.Project)
                                                        .Include(t => t.DeveloperUser)
                                                        .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                try
                {
                    ticket.Updated = DateTimeOffset.Now;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();

                    notification = new()
                    {
                        TicketId = ticket.Id,
                        Title = $"Ticket modified on project - {oldTicket.Project.Name}",
                        Message = $"Ticket: [{ticket.Id}]:{ticket.Title} updated by {user?.FullName}",
                        Created = DateTimeOffset.Now,
                        SenderId = user?.Id,
                        RecipientId = pm?.Id
                    };

                    if(pm != null)
                    {
                        await _notificationService.SaveNotificationAsync(notification);
                        await _notificationService.EmailNotificationAsync(notification, "New Ticket Added");
                    }
                    else
                    {
                        //Admin notification
                        await _notificationService.AdminsNotificationAsync(notification, companyId);
                    }

                    //Alert developer if ticket is modified
                    if(ticket.DeveloperUserId != null)
                    {
                        notification = new()
                        {
                            TicketId = ticket.Id,
                            Title = "A ticket assigned to you has been modififed",
                            Message = $"Ticket: [{ticket.Id}]:{ticket.Title} updated by {user?.FullName}",
                            Created = DateTimeOffset.Now,
                            SenderId = user?.Id,
                            RecipientId = ticket.DeveloperUserId
                        };

                        await _notificationService.SaveNotificationAsync(notification);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                //Add History
                Ticket newTicket = await _context.Ticket.Include(t => t.TicketPriority)
                                        .Include(t => t.TicketStatus)
                                        .Include(t => t.TicketType)
                                        .Include(t => t.Project)
                                        .Include(t => t.DeveloperUser)
                                        .AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticket.Id);

                await _historyService.AddHistoryAsync(oldTicket, newTicket, user.Id);

                return RedirectToAction(nameof(MyTickets));
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Name", ticket.DeveloperUserId);
            ViewData["OwnerUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.OwnerUserId);
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Name", ticket.ProjectId);
            ViewData["TicketPriorityId"] = new SelectList(_context.Set<TicketPriority>(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.Set<TicketStatus>(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.Set<TicketType>(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyTickets(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 5;

            var userId = _userManager.GetUserId(User);
            var devTickets = await _ticketService.GetAllTicketsByRoleAsync("Developer", userId);
            var subTickets = await _ticketService.GetAllTicketsByRoleAsync("Submitter", userId);
            var viewModel = new MyTicketsViewModel()
            {
                DevTickets = devTickets.OrderByDescending(t => t.Created).ToPagedList(pageNumber, pageSize),
                SubTickets = subTickets.OrderByDescending(t => t.Created).ToPagedList(pageNumber, pageSize)
            };

            decimal totalDevPages = ((decimal)(viewModel.DevTickets.Count() / (decimal)pageSize));
            decimal totalSubPages = ((decimal)(viewModel.SubTickets.Count() / (decimal)pageSize));
            ViewBag.TotalDevPages = Math.Ceiling(totalDevPages);
            ViewBag.TotalSubPages = Math.Ceiling(totalSubPages);

            ViewBag.devTickets = viewModel.DevTickets;
            ViewBag.subTickets = viewModel.SubTickets;
            ViewBag.pageNumber = pageNumber;
            
            
   


            return View( viewModel);
            //string myId = (await _userManager.GetUserAsync(User)).Id;
            ////get companyId
            //int companyId = User.Identity.GetCompanyId().Value;

            //var tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

            //var model = tickets.Where(t => t.OwnerUserId == myId);

            //return View(model);


        }

        [Authorize(Roles = "Administrator, ProjectManager, Developer")]
        [HttpGet]
        public async Task<IActionResult> CompanyTickets()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var model = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            return View(model);
        }

        [Authorize]
        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .Include(t => t.DeveloperUser)
                .Include(t => t.OwnerUser)
                .Include(t => t.Project)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.Id == id);
        }
    }
}
