using SMSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMSS.ViewModels
{
    public class CompanyLocationVM
    {
        public CompanyLocation CompanyLocation { get; set; }
        public LookupVM lookupVM { get; set; }
    }
}
