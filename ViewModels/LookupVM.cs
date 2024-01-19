using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMSS.Models;
using SMSS.Models.DataAnnotaions;

namespace SMSS.ViewModels
{
    public class LookupVM
    {
        public LookupVM()
        {
            this.Countries = new List<SelectListItem>();
            this.Provinves = new List<SelectListItem>();
            this.Cities = new List<SelectListItem>();
        }
        public List<Sector> Sectors { get; set; }
        public List<SelectListItem> Countries { get; set; }
        public List<SelectListItem> Provinves { get; set; }
        public List<SelectListItem> Cities { get; set; }
        public List<SelectListItem> Months { get; set; }


    }
}
