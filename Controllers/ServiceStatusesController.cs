using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinderApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Controllers
{
    public class ServiceStatusesController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;
        public ServiceStatusesController(ServiceFinderDBContext context)
        {
            _context = context;
        }
        [HttpGet("/GetServiceStatuses")]
        public async Task<ServiceResponse<List<ServiceStatus>>> Index()
        {
            return ServiceResponse<List<ServiceStatus>>.Ok(await _context.ServiceStatuses.ToListAsync(), "Status list successfully downloaded");
        }

        [HttpPost("/GetServiceStatus")]
        public async Task<ServiceResponse<ServiceStatus>> Details(Guid? id)
        {
            if (id == null)
            {
                return ServiceResponse<ServiceStatus>.Error("Id parameter is null");
            }

            var serviceStatus = await _context.ServiceStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceStatus == null)
            {
                return ServiceResponse<ServiceStatus>.Error("Id not match to any service status");
            }

            return ServiceResponse<ServiceStatus>.Ok(serviceStatus, "Status found");
        }

        [HttpPost("/CreateServiceStatus")]
        public async Task<ServiceResponse<bool>> Create(string status)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("Service status adding error");

            ServiceStatus serviceStatusToAdd = new ServiceStatus
            {
                Id = Guid.NewGuid(),
                Status = status

            };

            _context.Add(serviceStatusToAdd);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "Service status was added");
        }
        [HttpPost("/DeleteServiceStatus")]
        public async Task<ServiceResponse<bool>> Delete(string serviceStatus)
        {
            var status = await _context.ServiceStatuses.Where(row => row.Status == serviceStatus).FirstOrDefaultAsync();
            if (status == null)
            {
                return ServiceResponse<bool>.Error("Service status not found");
            }

            _context.Remove(status);
            await _context.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Service status succesfully deleted");
        }

        [HttpGet("/IfServiceStatusExist")]
        public async Task<ServiceResponse<bool>> ServiceStatusExists(Guid id)
        {
            bool isExist = await _context.ServiceStatuses.AnyAsync(e => e.Id == id);
            if (isExist)
                return ServiceResponse<bool>.Ok(true, "Service status exist");
            return ServiceResponse<bool>.Error("Service status don't exist");
        }
    }
}
