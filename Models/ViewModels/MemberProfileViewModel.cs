using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Models.ViewModels
{
    public class MemberProfileViewModel
    {
        public BTUser Member { get; set; }
        public List<Project> Projects {get; set;}
        public List<Ticket> Tickets { get; set; }
        public List<TicketComment> Comments { get; set; }
    }
}
