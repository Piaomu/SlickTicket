using SlickTicket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Services.Interfaces
{
    public interface IBTHistoryService
    {
       Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId);

        Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId);

        Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId);

    }
}
