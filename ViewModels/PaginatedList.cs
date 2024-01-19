using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMSS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SMSS.ViewModels
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

      
        [BindProperty]
        public CompanyJob CompanyJob { get; set; }

        public List<SelectListItem> Countries { get; set; }


       
        public PaginatedList()
        {
            CompanyJob = new CompanyJob();
            Countries = new List<SelectListItem>();
            CompanyLocations = new List<CompanyLocationVM>();
            CompanyProfile = new CompanyProfile();
            RegisteredUser = new RegisteredUser();
            AllApplicantsVM = new AllApplicantsVM();
            JobApplicantsList = new JobApplicantsList();
            CompanyTestimonialVM = new CompanyTestimonialVM();
        }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = count;
            TotalPages = pageSize !=0 ?(int)Math.Ceiling(count / (double)pageSize) : 1;

            this.AddRange(items);
        }

        
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            dynamic items;
           if (pageSize != 0)
            {
                 items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            } else
            {
                 items = await source.ToListAsync();
            }
            
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public CompanyProfile CompanyProfile { get; set; }
        public virtual List<CompanyLocationVM> CompanyLocations { get; set; }
        public CompanyLocation CompanyLocation { get; set; }

        public RegisteredUser RegisteredUser { get; set; }

        public AllApplicantsVM AllApplicantsVM { get; set; }

        public JobApplicantsList JobApplicantsList { get; set; }

        public CompanyTestimonialVM CompanyTestimonialVM { get; set; }

    }
}
