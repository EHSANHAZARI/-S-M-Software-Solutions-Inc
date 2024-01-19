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
    public class JobSectorVM
    {
        public int Id { get; set; }
        public string SectorName { get; set; }

        public List<Sector> Sectors { get; set; }
    }
}
