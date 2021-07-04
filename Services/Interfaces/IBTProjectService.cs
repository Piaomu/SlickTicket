using SlickTicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Services.Interfaces
{
    public interface IBTProjectService
    {
        public Task<List<Project>> GetAllProjectsByCompanyAsync(int companyId);
        public Task<bool> IsUserOnProject(string userId, int projectId);

        public Task<bool> AddUserToProjectAsync(string userId, int projectId);

        public Task RemoveUserFromProjectAsync(string userId, int projectId);

        public Task RemoveUsersFromProjectByRoleAsync(string userId, int projectId);

        public Task<List<Project>> ListUserProjectsAsync(string userId);

        public Task<List<Project>> GetAllProjectsByCompany(int companyId);

        public Task<List<Project>> GetArchivedProjectsByCompany(int companyId);

        public Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName);

        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role);

        public Task<BTUser> GetProjectManagerAsync(int projectId);

        public Task<bool> AddProjectManagerAsync(string userId, int projectId);

        public Task RemoveProjectManagerAsync(int projectId);

        public Task<List<BTUser>> GetMembersWithoutPMAsync(int projectId);

        public Task<List<BTUser>> UsersNotOnProjectAsync(int projectId, int companyId);

        public Task<List<BTUser>> DevelopersOnProjectAsync(int projectId);

        public Task<List<BTUser>> SubmittersOnProjectAsync(int projectId);
    }
}
