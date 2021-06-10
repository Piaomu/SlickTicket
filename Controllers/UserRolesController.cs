using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SlickTicket.Data;
using SlickTicket.Extensions;
using SlickTicket.Models;
using SlickTicket.Models.Enums;
using SlickTicket.Models.ViewModels;
using SlickTicket.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Controllers
{
    [Authorize(Roles="Administrator")]

    public class UserRolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _infoService;
        public UserRolesController(ApplicationDbContext context,
                                   IBTRolesService rolesService,
                                   UserManager<BTUser> userManager,
                                   IBTCompanyInfoService infoService)
        {
            _context = context;
            _userManager = userManager;
            _rolesService = rolesService;
            _infoService = infoService;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            var companyId = User.Identity.GetCompanyId();

            List<ManageUserRolesViewModel> model = new();

            //Am I trying to limit the Admin to Manage users roles only by their company?
            List<BTUser> companyUsers = await _infoService.GetAllMembersAsync((int)companyId);
            //TODO: CompanyUsers...little more work to do
            List<BTUser> users = _context.Users.ToList();

            foreach (var user in users)
            {
                ManageUserRolesViewModel vm = new();
                vm.BTUser = user;
                var selected = await _rolesService.ListUserRolesAsync(user);
                vm.Roles = new MultiSelectList(_context.Roles, "Name", "Name", selected);
                model.Add(vm);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            BTUser user = _context.Users.Find(member.BTUser.Id);

            IEnumerable<string> roles = await _rolesService.ListUserRolesAsync(user);
            
            await _rolesService.RemoveUserFromRolesAsync(user, roles);
            string userRole = member.SelectedRoles.FirstOrDefault();

            //What purpose is roleValue serving?
            if (Enum.TryParse(userRole, out Roles roleValue))
            { await _rolesService.AddUserToRoleAsync(user, userRole);
                return RedirectToAction("ManageUserRoles");
            }
            return RedirectToAction("ManageUserRoles");
        }
    }
}
