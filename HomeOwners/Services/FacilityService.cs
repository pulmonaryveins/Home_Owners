// HomeOwners/Services/FacilityService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Services
{
    public class FacilityService
    {
        private readonly HomeDbContext _context;

        public FacilityService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Facility>> GetAllFacilitiesAsync()
        {
            return await _context.Facilities.OrderBy(f => f.Name).ToListAsync();
        }

        public async Task<List<Facility>> GetActiveFacilitiesAsync()
        {
            return await _context.Facilities
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<Facility> GetFacilityByIdAsync(int id)
        {
            return await _context.Facilities.FindAsync(id);
        }

        public async Task CreateFacilityAsync(Facility facility)
        {
            facility.CreatedDate = DateTime.Now;
            _context.Facilities.Add(facility);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFacilityAsync(Facility facility)
        {
            facility.LastUpdated = DateTime.Now;
            _context.Facilities.Update(facility);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFacilityAsync(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null)
            {
                _context.Facilities.Remove(facility);
                await _context.SaveChangesAsync();
            }
        }

        // Alternatively, implement soft delete by setting IsActive to false
        public async Task DeactivateFacilityAsync(int id)
        {
            var facility = await _context.Facilities.FindAsync(id);
            if (facility != null)
            {
                facility.IsActive = false;
                facility.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}