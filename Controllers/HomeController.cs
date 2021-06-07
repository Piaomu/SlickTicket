using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SlickTicket.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using SlickTicket.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SlickTicket.Extensions;
using SlickTicket.Services.Interfaces;
using SlickTicket.Models.ViewModels;
using SlickTicket.Models.Enums;

namespace SlickTicket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyInfoService _infoService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly UserManager<BTUser> _userManager;

        public HomeController(ILogger<HomeController> logger,
                              ApplicationDbContext context,
                              IBTCompanyInfoService infoService,
                              IBTProjectService projectService,
                              IBTTicketService ticketService,
                              UserManager<BTUser> userManager)
        {
            _logger = logger;
            _context = context;
            _infoService = infoService;
            _projectService = projectService;
            _ticketService = ticketService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard() 
        {
            var userId = _userManager.GetUserId(User);
            var companyId = User.Identity.GetCompanyId().Value;
            var projects = await _projectService.GetAllProjectsByCompany(companyId);
            var tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            var members = await _infoService.GetAllMembersAsync(companyId);
            var viewModel = new DashboardViewModel()
            {
                Projects = projects,
                Tickets = tickets,
                Users = members
            };

            return View(viewModel);
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
