using Microsoft.EntityFrameworkCore;
using SlickTicket.Data;
using SlickTicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Services
{
    public class SearchService
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Ticket>> SearchActiveContent(string searchString)
        {
            var result = _context.Ticket.Where(t => t.Archived == false);

            if (!string.IsNullOrEmpty(searchString))
            {

                result = result.Where(t => t.Title.Contains(searchString) ||
                                           t.Description.Contains(searchString) ||
                                           t.DeveloperUser.FullName.Contains(searchString) ||
                                           t.OwnerUser.FullName.Contains(searchString) ||
                                           t.TicketType.Name.Contains(searchString) ||
                                           t.TicketStatus.Name.Contains(searchString) ||
                                           t.Comments.Any(c => c.Comment.Contains(searchString) ||
                                                               c.User.FullName.Contains(searchString)));
            }

            return await result.OrderByDescending(t => t.Created).ToListAsync();


        }

        public async Task<IEnumerable<Ticket>> SearchArchivedContent(string searchString)
        {
            var result = _context.Ticket.Where(t => t.Archived == true);

            if (!string.IsNullOrEmpty(searchString))
            {

                result = result.Where(t => t.Title.Contains(searchString) ||
                                           t.Description.Contains(searchString) ||
                                           t.DeveloperUser.FullName.Contains(searchString) ||
                                           t.OwnerUser.FullName.Contains(searchString) ||
                                           t.TicketType.Name.Contains(searchString) ||
                                           t.TicketStatus.Name.Contains(searchString) ||
                                           t.Comments.Any(c => c.Comment.Contains(searchString) ||
                                                               c.User.FullName.Contains(searchString)));
            }

            return await result.OrderByDescending(t => t.Created).ToListAsync(); ;


        }
    }
}
