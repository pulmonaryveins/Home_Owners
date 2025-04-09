// HomeOwners/Services/ServiceService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Services
{
    public class ServiceService
    {
        private readonly HomeDbContext _context;

        public ServiceService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetAllServicesAsync()
        {
            return await _context.Services.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<List<Service>> GetActiveServicesAsync()
        {
            return await _context.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<Service> GetServiceByIdAsync(int id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task CreateServiceAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateServiceAsync(Service service)
        {
            service.LastUpdated = System.DateTime.Now;
            _context.Attach(service).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }
    }
}