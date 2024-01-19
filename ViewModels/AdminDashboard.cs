using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class AdminDashboard
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Byte[] CompanyLogo { get; set; }
        public List<CompanyJobPostVM> CompanyPosts { get; set; }
        public List<ApplicantJobApplication> ApplicantJobApplications { get; set; }
        public List<SelectListItem> Companies { get; set; }
    }
}
