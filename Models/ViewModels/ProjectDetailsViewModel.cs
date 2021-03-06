using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Models.ViewModels
{
    public class ProjectDetailsViewModel
    {
        public BTUser BTUser { get; set; }
        public SelectList Roles { get; set; }
        public List<string> SelectedRoles { get; set; }
        public Project Project { get; set; }
    }
}
