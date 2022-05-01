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

            var services = await (from ser in _context.Services.Where(r=>r.IsArchival==false).Where(r=>r.IsPrivate == false)
                                  join serTyp in _context.ServiceTypes on ser.ServiceTypeId equals serTyp.Id
                                  where ser.ProviderId == providerID
                                  select new ServiceView
                                  {
                                      Id = ser.Id,
                                      Description = ser.Description,
                                      Duration = (int)ser.Duration,
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
                ServiceTypeId = servTypeId,
                Duration = service.Duration
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
        public async Task<ServiceResponse<bool>> Delete(Guid Id)
        {
            var service = await _context.Services.Where(row => row.Id == Id).FirstOrDefaultAsync();
            if (service == null)
            {
                return ServiceResponse<bool>.Error("Service not found");
            }
            service.IsArchival = true;
            _context.Update(service);
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

            var result = await (from prov in _context.Providers.Where(row => row.Id == ProviderId)
                                join serv in _context.Services.Where(r=>r.IsArchival == false) on prov.Id equals serv.ProviderId
                                join type in _context.ServiceTypes on serv.ServiceTypeId equals type.Id
                                where serv.IsPrivate == false
                                where serv.IsArchival == false
                                select new GetProvidersServicesView 
                                {
                                    ProviderLogo = prov.Logo,
                                    ProviderDescription = prov.Description,
                                    ProviderName = prov.Name,
                                    Phone = prov.Phone,
                                    Description = serv.Description,
                                    Id = serv.Id,
                                    Price = serv.Price,
                                    ProviderId = serv.ProviderId,
                                    ServiceName = serv.ServiceName,
                                    Duration = (int)serv.Duration,

                                    IsPriced= type.TypeName=="Priced"? true:false
                                }).ToListAsync();
            return ServiceResponse<List<GetProvidersServicesView>>.Ok(result, "Service exist");
        }
    }
}
