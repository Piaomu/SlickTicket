using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Models
{
    public class TicketHistory
    {
        // -- Primary Key -- //
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Updated Item")]
        public string Property { get; set; }

        [DisplayName("Previous")]
        public string OldValue { get; set; }

        [DisplayName("Current")]
        public string NewValue { get; set; }

        [DisplayName("Date Modified")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }

        [DisplayName("Description of Change")]
        public string Description { get; set; }


        // -- Navigational Properties -- //
        public virtual Ticket Ticket { get; set; }
        public BTUser User { get; set; }
    }
}
