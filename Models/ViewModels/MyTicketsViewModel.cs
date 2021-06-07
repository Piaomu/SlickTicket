using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Models.ViewModels
{
    public class MyTicketsViewModel
    {
        public IEnumerable<Ticket> DevTickets { get; set; }
        public IEnumerable<Ticket> SubTickets { get; set; }
    }
}
