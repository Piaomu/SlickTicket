using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Models
{
    public class Invite
    {
        public int Id { get; set; }
        [DisplayName("Date")]
        public DateTimeOffset InviteDate { get; set; }
        [DisplayName("Code")]
        public Guid CompanyToken { get; set; }
        [DisplayName("Company")]
        public int CompanyId { get; set; }
        [DisplayName("Project")]
        public int ProjectId { get; set; }
        [DisplayName("Invitor")]
        public string InvitorId { get; set; }
        [DisplayName("Invitee")]
        public string InviteeId { get; set; }
        [DisplayName("Email")]
        public string InviteeEmail { get; set; }
        [DisplayName("First Name")]
        public string InviteeFirstName { get; set; }
        [DisplayName("Last Name")]
        public string InviteeLastName { get; set; }
        
        public bool IsValid { get; set; }

        public virtual Company Company { get; set; }
        public virtual BTUser Invitor { get; set; }
        public virtual BTUser Invitee { get; set; }
        public virtual Project Project { get; set; }

    }
}
