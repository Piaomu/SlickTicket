using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SlickTicket.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlickTicket.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<SlickTicket.Models.Company> Company { get; set; }
        public DbSet<SlickTicket.Models.Invite> Invite { get; set; }
        public DbSet<SlickTicket.Models.Notification> Notification { get; set; }
        public DbSet<SlickTicket.Models.Project> Project { get; set; }
        public DbSet<SlickTicket.Models.ProjectPriority> ProjectPriority { get; set; }
        public DbSet<SlickTicket.Models.Ticket> Ticket { get; set; }
        public DbSet<SlickTicket.Models.TicketAttachment> TicketAttachment { get; set; }
        public DbSet<SlickTicket.Models.TicketComment> TicketComment { get; set; }
        public DbSet<SlickTicket.Models.TicketHistory> TicketHistory { get; set; }
        public DbSet<SlickTicket.Models.TicketPriority> TicketPriority { get; set; }
        public DbSet<SlickTicket.Models.TicketStatus> TicketStatus { get; set; }
        public DbSet<SlickTicket.Models.TicketType> TicketType { get; set; }
    }
}
