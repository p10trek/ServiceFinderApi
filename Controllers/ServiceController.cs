using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinderApi.Models;
using ServiceFinderApi.Models.RequestModels;
using ServiceFinderApi.Models.ViewModels;
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
        [Authorize(Roles = "Manager")]
        public async Task<ServiceResponse<List<ServiceView>>> Index()
        {
            var providerID = await (from user in _context.Users.Where(row => row.IsProvider == true)
                       .Where(row => row.Login == User.Identity.Name)
                                    join prov in _context.Providers on user.Id equals prov.UserId
                                    select prov).Select(r => r.Id).FirstOrDefaultAsync();

            var services = await (from ser in _context.Services
                                  join serTyp in _context.ServiceTypes on ser.ServiceTypeId equals serTyp.Id
                                  where ser.ProviderId == providerID
                                  select new ServiceView
                                  {
                                      Description = ser.Description,
                                      Price = ser.Price,
                                      ServiceName = ser.ServiceName,
                                      ServiceType = serTyp.TypeName
                                  }).ToListAsync();
            return ServiceResponse<List<ServiceView>>.Ok(services, "Service list successfully downloaded");
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
        [Authorize(Roles = "Manager")]
        public async Task<ServiceResponse<bool>> Create(CreateService service)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");

            var providerID = await (from user in _context.Users.Where(row => row.IsProvider == true)
                                   .Where(row => row.Login == User.Identity.Name)
                                   join prov in _context.Providers on user.Id equals prov.UserId
                                   select prov).Select(r => r.Id).FirstOrDefaultAsync();

            string serviceType = service.ServiceType ? "Priced" : "UnPriced";

            var servTypeId = await _context.ServiceTypes
                .Where(row => row.TypeName == serviceType)
                .Select(r => r.Id).FirstOrDefaultAsync();

            Service serviceToAdd = new Service
            {
                Id = Guid.NewGuid(),
                ServiceName = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                ProviderId = providerID,
                ServiceTypeId = servTypeId
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
            if (service.Price != editedService.Price)
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
        [HttpGet("/GetProviderServices")]
        public async Task<ServiceResponse<List<GetProvidersServicesView>>> GetProviderServices(Guid ProviderId)
        {
            var result = await _context.Services
                                 .Where(row => row.ProviderId == ProviderId).Select(row =>
                                    new GetProvidersServicesView
                                    {
                                        Description = row.Description,
                                        Id = row.Id,
                                        Price = row.Price,
                                        ProviderId = row.ProviderId,
                                        ServiceName = row.ServiceName,
                                        ServiceTypeId = row.ServiceTypeId
                                    }
                                ).ToListAsync();
            return ServiceResponse<List<GetProvidersServicesView>>.Ok(result, "Service exist");
        }
    }
}
