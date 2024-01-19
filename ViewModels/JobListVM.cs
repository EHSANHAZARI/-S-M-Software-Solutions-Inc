using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class JobListVM
    {
        public JobListVM()
        {
            this.CompanyJobs = new List<CompanyJob>();
            //  Sectors = new List<sector>();
            this.JobModes = new List<JobMode>();

        }

        public List<CompanyJob> CompanyJobs { get; set; }
        public List<JobMode> JobModes { get; set; }
        public List<Sector> Sectors { get; set; }
        
    }
}
