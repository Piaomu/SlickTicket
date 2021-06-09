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
    public class BTHistoryService : IBTHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            // NEW TICKET HAS BEEN ADDED
            if (oldTicket == null && newTicket != null)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    Property = "",
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = $"New Ticket, {newTicket.Title} Created"
                };
                await _context.TicketHistory.AddAsync(history);
                await _context.SaveChangesAsync();
            }
            else
            {
                if(oldTicket.Title != newTicket.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket title: {newTicket.Title}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket Description: {newTicket.Description}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Type",
                        OldValue = oldTicket.TicketType.Name,
                        NewValue = newTicket.TicketType.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket Type: {newTicket.TicketType.Name}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Priority",
                        OldValue = oldTicket.TicketPriority.Name,
                        NewValue = newTicket.TicketPriority.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket Priority: {newTicket.TicketPriority.Name}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Ticket Status",
                        OldValue = oldTicket.TicketStatus.Name,
                        NewValue = newTicket.TicketStatus.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New ticket Priority: {newTicket.TicketStatus.Name}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newTicket.DeveloperUser?.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New developer: {newTicket.DeveloperUser.FullName}"
                    };
                    await _context.TicketHistory.AddAsync(history);
                }

                //Save the TicketHistory DataBaseSet to the db
                await _context.SaveChangesAsync();
            }

        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            Company company = await _context.Company.Include(c => c.Projects)
                                                    .ThenInclude(p => p.Tickets)
                                                        .ThenInclude(t => t.History)
                                                    .FirstOrDefaultAsync(c => c.Id == companyId);

            List<Ticket> tickets = company.Projects.SelectMany(t => t.Tickets).ToList();

            List<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();

            return ticketHistories;
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId)
        {
            Project project = await _context.Project.Include(p => p.Tickets)
                                                    .ThenInclude(t => t.History)
                                                    .FirstOrDefaultAsync(p => p.Id == projectId);

            List<TicketHistory> ticketHistories = project.Tickets.SelectMany(t => t.History).ToList();

            return ticketHistories;

        }
    }
}
