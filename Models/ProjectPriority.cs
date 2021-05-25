using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Models
{
    public class ProjectPriority
    {
        // Primary Key
        public int Id { get; set; }
        [DisplayName("Project Priority")]
        public string Name { get; set; }
    }
}
