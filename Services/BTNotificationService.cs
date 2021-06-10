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
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IBTCompanyInfoService _companyInfoService;
        public BTNotificationService(ApplicationDbContext context,
                                     IEmailSender emailSender,
                                     IBTCompanyInfoService companyInfoService)
        {
            _context = context;
            _emailSender = emailSender;
            _companyInfoService = companyInfoService;
            

        }
        public async Task AdminsNotificationAsync(Notification notification, int companyId)
        {
            try
            {
                //Get company admin(s)
                List<BTUser> admins = await _companyInfoService.GetMembersInRoleAsync("Administrator", companyId);
                
                foreach(BTUser btuser in admins)
                {
                    notification.RecipientId = btuser.Id;

                    await EmailNotificationAsync(notification, notification.Title);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task EmailNotificationAsync(Notification notification, string emailSubject)
        {
            BTUser user = await _context.Users.FindAsync(notification.RecipientId);

            //Send Email
            string btUserEmail = user.Email;
            string message = notification.Message;
            try
            {
                await _emailSender.SendEmailAsync(btUserEmail, emailSubject, message);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        {
            List<Notification> notifications = await _context.Notification
                                                             .Include(n => n.Recipient)
                                                             .Include(n => n.Sender)
                                                             .Include(n => n.Ticket)
                                                                .ThenInclude(t => t.Project)
                                                             .Where(n => n.RecipientId == userId).ToListAsync();

            return notifications;
        }

        public async Task<List<Notification>> GetSentNotificationsAsync(string userId)
        {
            List<Notification> notifications = await _context.Notification
                                                             .Include(n => n.Recipient)
                                                             .Include(n => n.Sender)
                                                             .Include(n => n.Ticket)
                                                                .ThenInclude(t => t.Project)
                                                             .Where(n => n.SenderId == userId).ToListAsync();

            return notifications;
        }

        public async Task MembersNotificationAsync(Notification notification, List<BTUser> members)
        {
            try
            {
                foreach (BTUser user in members)
                {
                    notification.RecipientId = user.Id;

                    await EmailNotificationAsync(notification, notification.Title);
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task SaveNotificationAsync(Notification notification)
        {
            try {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            } 
            catch 
            {
                throw;
            }
        }

        public Task SMSNotificationAsync(string phone, Notification notification)
        {
            throw new NotImplementedException();
        }
    }
}
