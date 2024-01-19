using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class PostJobVM
    {
        [BindProperty]
        public CompanyJob CompanyJob { get; set; }

       
        [Display(Name = "Terms and Conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please Accept the Terms & Conditions!")]
        public bool TermsAndConditions { get; set; }

        public List<SelectListItem> Countries { get; set; }
        public List<SelectListItem> Provices { get; set; }
        public List<SelectListItem> Cites { get; set; }
        public List<SelectListItem> Sectors { get; set; }
        public List<SelectListItem> ProvinceDemoFileAttachments { get; set; }
        public List<SelectListItem> JobModes { get; set; }

        public PostJobVM()
        {
            CompanyJob = new CompanyJob();
            Countries = new List<SelectListItem>();
            Sectors = new List<SelectListItem>();
            ProvinceDemoFileAttachments = new List<SelectListItem>();
            JobModes = new List<SelectListItem>();
        }
        [Display(Name = "JobMatrix File")]
        public IFormFile FileInfo { get; set; }

        




        //[Display(Name = "Demo JobMatrix File")]
        //public IFormFile DemoFileInfo { get; set; }
    }
}
