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
using Microsoft.AspNetCore.Authorization;
using System.Drawing;

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

        [Authorize]
        public async Task<IActionResult> Dashboard() 
        {
            var userId = _userManager.GetUserId(User);
            var companyId = User.Identity.GetCompanyId().Value;
            var projects = await _projectService.GetAllProjectsByCompany(companyId);
            var tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);
            var members = await _infoService.GetAllMembersAsync(companyId);
            var company = _context.Company.FirstOrDefault(c => c.Id == companyId);
            
            var viewModel = new DashboardViewModel()
            {
                Projects = projects,
                Tickets = tickets,
                Users = members,
                Company = company
            };

            return View(viewModel);
        }

        [Authorize]
        public async Task<JsonResult> DonutMethod()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Random rnd = new();

            List<Project> projects = (await _projectService.GetAllProjectsByCompany(companyId)).OrderBy(p => p.Id).ToList();

            DonutViewModel chartData = new();
            chartData.labels = projects.Select(p => p.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> tickets = new();
            List<string> colors = new();

            foreach (Project prj in projects)
            {
                tickets.Add(prj.Tickets.Count());

                // This code will randomly select a color for each element of the data 
                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = tickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }

        [Authorize]
        public async Task<JsonResult> DonutMethod2()
        {
            int companyId = User.Identity.GetCompanyId().Value;
            Random rnd = new();

            List<Ticket> tickets = (await _ticketService.GetAllTicketsByCompanyAsync(companyId)).OrderBy(t => t.Id).ToList();
            List<TicketStatus> statuses = _context.TicketStatus.ToList();

            DonutViewModel chartData = new();
            chartData.labels = tickets.Select(t => t.TicketStatus.Name).Distinct().ToArray();
            //chartData.labels = statuses.Select(s => s.Name).ToArray();

            List<SubData> dsArray = new();
            List<int> numberOfTickets = new();
            List<string> colors = new();

            foreach (var status in chartData.labels.ToList())
            {
                var statusId = statuses.FirstOrDefault(s => s.Name == status).Id;
                numberOfTickets.Add(tickets.Where(t => t.TicketStatusId == statusId).Count());

                // This code will randomly select a color for each element of the data 
                Color randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                string colorHex = string.Format("#{0:X6}", randomColor.ToArgb() & 0X00FFFFFF);

                colors.Add(colorHex);
            }

            SubData temp = new()
            {
                data = numberOfTickets.ToArray(),
                backgroundColor = colors.ToArray()
            };
            dsArray.Add(temp);

            chartData.datasets = dsArray.ToArray();

            return Json(chartData);
        }

        public IActionResult Landing()
        {
            return View();
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
