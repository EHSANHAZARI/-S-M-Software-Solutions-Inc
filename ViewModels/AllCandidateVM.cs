using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class AllCandidateVM
    {
        public PaginatedList<ApplicantProfile> ApplicantProfiles { get; set; }
        public List<SelectListItem> SectorsList { get; set; }
        
        public List<Sector> Sectors { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Country { get; set; }
        public int? Province { get; set; }
        public int? City { get; set; }
        public String Name { get; set; }
        public String QueryString { get; set; }
        public List<SelectListItem> Countries { get; set; }
     

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

    }
}
