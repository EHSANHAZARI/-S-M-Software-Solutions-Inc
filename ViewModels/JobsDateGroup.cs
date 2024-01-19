using SMSS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class JobsDateGroup
    {
        [DataType(DataType.Date)]
        public DateTime? applicationDate { get; set; }
        public int jobCount { get; set; }
        
    }
}
