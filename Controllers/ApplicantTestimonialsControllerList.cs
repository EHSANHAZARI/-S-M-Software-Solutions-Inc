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
    public class ApplicantTestimonialsControllerList : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicantTestimonialsControllerList(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ApplicantTestimonialsControllerList
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicantTestimonial>>> GetApplicantTestimonials()
        {
            return Ok(await _context.ApplicantTestimonials.ToListAsync());
        }

        // GET: api/ApplicantTestimonialsControllerList/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicantTestimonial>> GetApplicantTestimonial(int id)
        {
            var applicantTestimonial = await _context.ApplicantTestimonials.FindAsync(id);

            if (applicantTestimonial == null)
            {
                return NotFound();
            }

            return applicantTestimonial;
        }

        // PUT: api/ApplicantTestimonialsControllerList/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutApplicantTestimonial(int id, ApplicantTestimonial applicantTestimonial)
        {
            if (id != applicantTestimonial.Id)
            {
                return BadRequest();
            }

            _context.Entry(applicantTestimonial).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicantTestimonialExists(id))
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

        // POST: api/ApplicantTestimonialsControllerList
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ApplicantTestimonial>> PostApplicantTestimonial(ApplicantTestimonial applicantTestimonial)
        {
            _context.ApplicantTestimonials.Add(applicantTestimonial);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetApplicantTestimonial", new { id = applicantTestimonial.Id }, applicantTestimonial);
        }

        // DELETE: api/ApplicantTestimonialsControllerList/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplicantTestimonial(int id)
        {
            var applicantTestimonial = await _context.ApplicantTestimonials.FindAsync(id);
            if (applicantTestimonial == null)
            {
                return NotFound();
            }

            _context.ApplicantTestimonials.Remove(applicantTestimonial);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ApplicantTestimonialExists(int id)
        {
            return _context.ApplicantTestimonials.Any(e => e.Id == id);
        }
    }
}
