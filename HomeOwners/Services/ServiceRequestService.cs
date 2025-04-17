// HomeOwners/Services/ServiceRequestService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeOwners.Areas.Identity.Data;
using HomeOwners.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Services
{
    public class ServiceRequestService
    {
        private readonly HomeDbContext _context;

        public ServiceRequestService(HomeDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAllServiceRequestsAsync()
        {
            var allRequests = await _context.ServiceRequests.ToListAsync();
            _context.ServiceRequests.RemoveRange(allRequests);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ServiceRequest>> GetAllServiceRequestsAsync()
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Service)
                .Include(sr => sr.ServicePersonnel)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByUserIdAsync(string userId)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Service)
                .Include(sr => sr.ServicePersonnel)
                .Where(sr => sr.UserId == userId)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ServiceRequest>> GetPendingServiceRequestsAsync()
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Service)
                .Include(sr => sr.ServicePersonnel)
                .Where(sr => sr.Status == ServiceRequestStatus.Pending)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();
        }

        public async Task<ServiceRequest> GetServiceRequestByIdAsync(int id)
        {
            return await _context.ServiceRequests
                .Include(sr => sr.Service)
                .Include(sr => sr.ServicePersonnel)
                .FirstOrDefaultAsync(sr => sr.Id == id);
        }

        public async Task<bool> HasActiveServiceRequestsAsync(string userId)
        {
            return await _context.ServiceRequests
                .AnyAsync(sr => sr.UserId == userId &&
                         (sr.Status == ServiceRequestStatus.Pending ||
                          sr.Status == ServiceRequestStatus.Approved) &&
                         sr.RequestDate >= DateTime.Today);
        }

        public async Task CreateServiceRequestAsync(ServiceRequest serviceRequest)
        {
            serviceRequest.CreatedDate = DateTime.Now;
            serviceRequest.Status = ServiceRequestStatus.Pending;

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateServiceRequestStatusAsync(int id, ServiceRequestStatus status)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest != null)
            {
                serviceRequest.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task AssignTeamToServiceRequestAsync(int requestId, int personnelId)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(requestId);
            if (serviceRequest != null)
            {
                serviceRequest.ServicePersonnelId = personnelId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteServiceRequestAsync(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest != null)
            {
                _context.ServiceRequests.Remove(serviceRequest);
                await _context.SaveChangesAsync();
            }
        }
    }
}