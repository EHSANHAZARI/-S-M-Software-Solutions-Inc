using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class CompanyJobPostVM
    {
        [DataType(DataType.Date)]
        public DateTime? jobPostDate { get; set; }
        public int jobCount { get; set; }
    }
}
