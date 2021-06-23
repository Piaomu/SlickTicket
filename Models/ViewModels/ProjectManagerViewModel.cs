using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlickTicket.Models.ViewModels
{
    public class ProjectManagerViewModel
    {
        public Project Project { get; set; } = new();
        public SelectList Users { get; set; } // populate list box
        public string SelectedUser { get; set; }// receives selected user
    }
}
