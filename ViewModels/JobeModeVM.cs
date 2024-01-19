using Microsoft.AspNetCore.Http;
using SMSS.Models;
using System.Collections.Generic;

namespace SMSS.ViewModels
{
    public class JobeModeVM
    {
       public int Id { get; set; }
        public string JobModeName { get; set; }

        public List<JobMode> JobModes { get; set; }
    }
}
