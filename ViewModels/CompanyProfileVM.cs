using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using SMSS.Models.DataAnnotaions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class CompanyProfileVM
    {
        public CompanyProfileVM()
        {
            CompanyLocations =  new List<CompanyLocationVM>();
            CompanyProfile = new CompanyProfile();

            CandidateNames = new List<CandidateVM>();
            Sectors = new List<JobSectorVM>();
        }

        
        public CompanyProfile  CompanyProfile { get; set; }
        public virtual List<CompanyLocationVM> CompanyLocations { get; set; }
        public CompanyLocation  CompanyLocation { get; set; }
        public List<SelectListItem> Countries { get; set; }

        public virtual List<CandidateVM> CandidateNames { get; set; }

        //public virtual List<Sector> Sectors { get; set; }

        public virtual List<JobSectorVM> Sectors { get; set; }
    }
}
