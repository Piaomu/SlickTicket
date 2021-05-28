using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SlickTicket.Data;
using SlickTicket.Models;
using SlickTicket.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _infoService;
        private readonly IBTRolesService _roleService;

        public BTProjectService(ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager, 
            UserManager<BTUser> userManager,
            IBTCompanyInfoService bTCompanyInfoService,
            IBTRolesService bTRoleService)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _infoService = bTCompanyInfoService;
            _roleService = bTRoleService;
        }
        public async Task<bool> IsUserOnProject(string userId, int projectId)
        {
            Project project = await _context.Project
                                    .FirstOrDefaultAsync(p => p.Id == projectId);

            bool result = project.Members.Any(u => u.Id == userId);

            return result;
        }

        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            try
            {
                BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user is not null)
                {
                    Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
                    if (!await IsUserOnProject(userId, projectId))
                    {
                        try
                        {
                            project.Members.Add(user);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error Adding user to project. --> {ex.Message}");
                return false;
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            //WRITE THIS SECOND
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string userId, int projectId)
        {
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId, role);
                Project project = await _context.Project.FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser btUser in members)
                {
                    try
                    {
                        project.Members.Remove(btUser);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error Removing users from project. ==> {ex.Message}");
            }
        }

        public async Task<List<Project>> ListUserProjectsAsync(string userId)
        {
            try
            {
                List<Project> userProjects = (await _context.Users
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Company)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Members)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.OwnerUser)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                              .FirstOrDefaultAsync(u => u.Id == userId)).Projects.ToList();

            return userProjects;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"*** ERROR *** - Error getting user projects list. --> {ex.Message}");
                throw;
            }
        }
        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            List<Project> projects = new();

            projects = await _context.Project.Include(p => p.Members)
                                 .Include(p => p.ProjectPriority)
                                 .Include(p => p.Tickets)
                                    .ThenInclude(t => t.OwnerUser)
                                 .Include(p => p.Tickets)
                                    .ThenInclude(t => t.DeveloperUser)
                                 .Include(p => p.Tickets)
                                    .ThenInclude(t => t.Comments)
                                 .Include(p => p.Tickets)
                                    .ThenInclude(t => t.Attachments)
                                 .Include(p => p.Tickets)
                                    .ThenInclude(t => t.History)
                                 .Include(p => p.Tickets)
                                    .ThenInclude(t => t.TicketPriority)
                                 .Include(p => p.Tickets)
                                    .ThenInclude(t => t.TicketStatus)
                                 .Include(p => p.Tickets)
                                    .ThenInclude(t => t.TicketType)
                                 .Where(p => p.CompanyId == companyId).ToListAsync();

            return projects;
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        { 
            //0. Instantiate a list of Projects
            List<Project> projects = new();

            //1. GetAllProjectsByCompany()
            projects = await GetAllProjectsByCompany(companyId);

            //2. Check if archived == true

            //3. return projects (where archived == true;)
            return projects.Where(p => p.Archived == true).ToList();
        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            List<Project> projects = new();

            projects = await GetAllProjectsByCompany(companyId)
                             .Where(p => p.ProjectPriority == priorityName).ToListAsync();

            return projects;
        }
        
        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        { //WRITE THIS FIRST
            Project project = await _context.Project
                              .Include(p => p.Members)
                              .FirstOrDefaultAsync(m => m.Id == projectId);
            
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Project
                              .Include(p => p.Members)
                              .FirstOrDefaultAsync(u => u.Id == projectId);
            foreach (BTUser member in project?.Members)
            {
                if(await _roleService.IsUserInRoleAsync(member, "ProjectManager"))
                {
                    return member;
                }
            }
            return null;
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {

        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {

        }

        public async Task<List<BTUser>> GetMembersWithoutPMAsync(int projectId)
        {
            List<BTUser> developers = await DevelopersOnProjectAsync(projectId);
            List<BTUser> submitters = await SubmittersOnProjectAsync(projectId);
            List<BTUser> administrators = await GetProjectMembersByRoleAsync(projectId, "Administrator");

            List<BTUser> teamMembers = developers.Concat(submitters).Concat(administrators).ToList();

            return teamMembers;
        }

        public async Task<List<BTUser>> UsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<BTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId) && u.CompanyId == companyId).ToListAsync();

            return users;
        }

        public async Task<List<BTUser>> DevelopersOnProjectAsync(int projectId)
        {
            Project project = await _context.Project
                             .Include(p => p.Members)
                             .FirstOrDefaultAsync(u => u.Id == projectId);

            List<BTUser> developers = new();

            foreach (var user in project.Members)
            {
                if (await _roleService.IsUserInRoleAsync(user, "Developer"))
                {
                    developers.Add(user);
                }
            }
            return developers;
        }

        public async Task<List<BTUser>> SubmittersOnProjectAsync(int projectId)
        {
            Project project = await _context.Project
                              .Include(p => p.Members)
                              .FirstOrDefaultAsync(u => u.Id == projectId);
            // instantiate a list of submitters
            List<BTUser> submitters = new();

            foreach (var user in project.Members)
            {
                if (await _roleService.IsUserInRoleAsync(user, "Submitter"))
                {
                    submitters.Add(user);
                }
            }
            return submitters;
        }
    }
}
