using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMSS.Data;
using SMSS.Models;

namespace SMSS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyJobListController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CompanyJobListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CompanyJobList
        [HttpGet]
        //public async Task<ActionResult<IEnumerable<CompanyJob>>> GetCompanyJobs()
        //{
        //    return await _context.CompanyJobs.Where(cj => cj.ExpireDate > DateTime.Now).OrderBy(cj => cj.Id).ToListAsync();
        //}

        public async Task<ActionResult<IEnumerable<CompanyJob>>> GetCompanyJobs()
        {
            return Ok(await _context.CompanyJobs.Where(cj => cj.ExpireDate > DateTime.Now).OrderBy(cj => cj.Id).ToListAsync());
        }

        // GET: api/CompanyJobList/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyJob>> GetCompanyJob(int id)
        {
            var companyJob = await _context.CompanyJobs.FindAsync(id);

            if (companyJob == null)
            {
                return NotFound();
            }

            return companyJob;
        }

        // PUT: api/CompanyJobList/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompanyJob(int id, CompanyJob companyJob)
        {
            if (id != companyJob.Id)
            {
                return BadRequest();
            }

            _context.Entry(companyJob).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyJobExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CompanyJobList
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CompanyJob>> PostCompanyJob(CompanyJob companyJob)
        {
            _context.CompanyJobs.Add(companyJob);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompanyJob", new { id = companyJob.Id }, companyJob);
        }

        // DELETE: api/CompanyJobList/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompanyJob(int id)
        {
            var companyJob = await _context.CompanyJobs.FindAsync(id);
            if (companyJob == null)
            {
                return NotFound();
            }

            _context.CompanyJobs.Remove(companyJob);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompanyJobExists(int id)
        {
            return _context.CompanyJobs.Any(e => e.Id == id);
        }
    }
}
