using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SlickTicket.Data;
using SlickTicket.Models;
using SlickTicket.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Services
{
    public class BTInviteService : IBTInviteService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _projectService;
        private readonly IEmailSender _emailService;

        public BTInviteService(ApplicationDbContext context,
                               IBTProjectService projectService,
                               IEmailSender emailService)
        {
            _context = context;
            _projectService = projectService;
            _emailService = emailService;
        }
        public async Task<bool> AcceptInviteAsync(Guid? token, string userId)
        {
            Invite invite = await _context.Invite.FirstOrDefaultAsync(i => i.CompanyToken == token);

            if( invite == null)
            {
                return false;
            }

            try
            {
                invite.InviteeId = userId;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> AnyInviteAsync(Guid token, string email)
        {
            return await _context.Invite.AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email && i.IsValid == true);
        }

        public async Task<Invite> GetInviteAsync(Guid token, string email)
        {
            Invite invite = await _context.Invite.Include(i => i.Company)
                                                 .Include(i => i.Project)
                                                 .Include(i => i.Invitor)
                                                 .FirstOrDefaultAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

            return invite;
        }

        public async Task<Invite> GetInviteAsync(int id)
        {
            Invite invite = await _context.Invite.Include(i => i.Company)
                                                 .Include(i => i.Project)
                                                 .Include(i => i.Invitor)
                                                 .FirstOrDefaultAsync(i => i.Id == id);

            return invite;
        }

        public async Task<bool> ValidateInviteCodeAsync(Guid? token)
        {
            if (token == null)
            {
                return false;
            }

            Invite invite = await _context.Invite.FirstOrDefaultAsync(i => i.CompanyToken == token);

            if ((DateTimeOffset.Now - (await _context.Invite.FirstOrDefaultAsync(i => i.CompanyToken == token)).InviteDate).TotalDays <= 7)
            {
                bool result = (await _context.Invite.FirstOrDefaultAsync(i => i.CompanyToken == token)).IsValid;
                return result;
            }
            else
            {
                return false;
            }
        }
    }
}
