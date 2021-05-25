using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public DateTimeOffset? Archived { get; set; }
        public int ProjectId { get; set; }
        public int StatusId { get; set; }
        public int TicketTypeId { get; set; }
        public string OwnerUserId { get; set; }
        public string DeveloperUserId { get; set; }

        // -- Navigation Properties -- //
        public virtual Project Project { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual BTUser OwnerUser { get; set; }
        public virtual BTUser DeveloperUser { get; set; }
        public virtual ICollection<TicketAttachment> Attachment { get; set; }
        public virtual ICollection<TicketComment> Comment { get; set; }
        public virtual History History { get; set; }
        public virtual ICollection<Notifications> Notifications { get; set; }

    }

}
