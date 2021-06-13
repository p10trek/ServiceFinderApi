using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinderApi.Models;
using ServiceFinderApi.Models.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Controllers
{
    public class ServiceController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;
        public ServiceController(ServiceFinderDBContext context)
        {
            _context = context;
        }

        [HttpGet("/GetServices")]
        public async Task<ServiceResponse<List<Service>>> Index()
        {
            return ServiceResponse<List<Service>>.Ok(await _context.Services.ToListAsync(), "Service list successfully downloaded");
        }

        [HttpPost("/GetService")]
        public async Task<ServiceResponse<Service>> Details(Guid? id)
        {
            if (id == null)
            {
                return ServiceResponse<Service>.Error("Id parameter is null");
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.Id == id);
            if (service == null)
            {
                return ServiceResponse<Service>.Error("Id not match to any service");
            }

            return ServiceResponse<Service>.Ok(service, "Service found");
        }

        [HttpPut("/CreateService")]
        public async Task<ServiceResponse<bool>> Create(CreateService service)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");

            Service serviceToAdd = new Service
            {
                Id = Guid.NewGuid(),
                ServiceName = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                ProviderId = service.ProviderId,
                ServiceTypeId = service.ServiceTypeId
            };

            _context.Add(serviceToAdd);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "Service was added");
        }

        [HttpPost("/EditService")]
        public async Task<ServiceResponse<bool>> Edit(EditService editedService)
        {
            var service = await _context.Services
                                        .Where(row => row.Id == editedService.Id)
                                        .FirstOrDefaultAsync();
            if (service == null)
            {
                return ServiceResponse<bool>.Error("Service not Found");
            }
            if (!String.IsNullOrEmpty(editedService.Price))
                service.Price = editedService.Price;
            if (!String.IsNullOrEmpty(editedService.ServiceName))
                service.ServiceName = editedService.ServiceName;
            if (editedService.ServiceTypeId != Guid.Empty)
                service.ServiceTypeId = editedService.ServiceTypeId;
            if (!String.IsNullOrEmpty(editedService.Description))
                service.Description = editedService.Description;

            _context.Update(service);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "Service edited corectly");
        }
       
        [HttpDelete("/DeleteService")]
        public async Task<ServiceResponse<bool>> Delete(string serviceName)
        {
            var service = await _context.Services.Where(row => row.ServiceName == serviceName).FirstOrDefaultAsync();
            if (service == null)
            {
                return ServiceResponse<bool>.Error("Service not found");
            }

            _context.Remove(service);
            await _context.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Service succesfully deleted");
        }

        [HttpGet("/IfServiceExist")]
        public async Task<ServiceResponse<bool>> ServiceExists(Guid id)
        {
            bool isExist = await _context.Services.AnyAsync(e => e.Id == id);
            if (isExist)
                return ServiceResponse<bool>.Ok(true, "Service exist");
            return ServiceResponse<bool>.Error("Service don't exist");
        }
    }
}
