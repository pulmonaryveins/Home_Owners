// HomeOwners/Services/ServicePersonnelService.cs
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeOwners.Services
{
    public class ServicePersonnelService
    {
        private readonly HomeDbContext _context;

        public ServicePersonnelService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task<List<ServicePersonnel>> GetAllServicePersonnelAsync()
        {
            return await _context.ServicePersonnel
                .Include(sp => sp.Service)
                .ToListAsync();
        }

        public async Task<List<ServicePersonnel>> GetServicePersonnelByServiceIdAsync(int serviceId)
        {
            return await _context.ServicePersonnel
                .Include(sp => sp.Service)
                .Where(sp => sp.ServiceId == serviceId)
                .ToListAsync();
        }

        public async Task<ServicePersonnel> GetServicePersonnelByIdAsync(int id)
        {
            return await _context.ServicePersonnel
                .Include(sp => sp.Service)
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<int> CreateServicePersonnelAsync(ServicePersonnel servicePersonnel)
        {
            _context.ServicePersonnel.Add(servicePersonnel);
            await _context.SaveChangesAsync();
            return servicePersonnel.Id;
        }

        public async Task UpdateServicePersonnelAsync(ServicePersonnel servicePersonnel)
        {
            _context.ServicePersonnel.Update(servicePersonnel);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteServicePersonnelAsync(int id)
        {
            var servicePersonnel = await _context.ServicePersonnel.FindAsync(id);
            if (servicePersonnel != null)
            {
                _context.ServicePersonnel.Remove(servicePersonnel);
                await _context.SaveChangesAsync();
            }
        }
    }
}