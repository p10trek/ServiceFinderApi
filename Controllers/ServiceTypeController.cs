using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinderApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Controllers
{
    public class ServiceTypeController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;
        public ServiceTypeController(ServiceFinderDBContext context)
        {
            _context = context;
        }
        [HttpGet("/GetServiceTypees")]
        public async Task<ServiceResponse<List<ServiceType>>> Index()
        {
            return ServiceResponse<List<ServiceType>>.Ok(await _context.ServiceTypes.ToListAsync(), "Service type list successfully downloaded");
        }

        [HttpPost("/GetServiceType")]
        public async Task<ServiceResponse<ServiceType>> Details(Guid? id)
        {
            if (id == null)
            {
                return ServiceResponse<ServiceType>.Error("Id parameter is null");
            }

            var serviceType = await _context.ServiceTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceType == null)
            {
                return ServiceResponse<ServiceType>.Error("Id not match to any service type");
            }

            return ServiceResponse<ServiceType>.Ok(serviceType, "Service type found");
        }

        [HttpPost("/CreateServiceType")]
        public async Task<ServiceResponse<bool>> Create(string typeName)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("Service type adding error");

            ServiceType serviceTypeToAdd = new ServiceType
            {
                Id = Guid.NewGuid(),
                TypeName = typeName

            };

            _context.Add(serviceTypeToAdd);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "Service type was added");
        }
        [HttpPost("/DeleteServiceType")]
        public async Task<ServiceResponse<bool>> Delete(string serviceType)
        {
            var Type = await _context.ServiceTypes.Where(row => row.TypeName == serviceType).FirstOrDefaultAsync();
            if (Type == null)
            {
                return ServiceResponse<bool>.Error("Service type not found");
            }

            _context.Remove(Type);
            await _context.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Service type succesfully deleted");
        }

        [HttpGet("/IfServiceTypeExist")]
        public async Task<ServiceResponse<bool>> ServiceTypeExists(Guid id)
        {
            bool isExist = await _context.ServiceTypes.AnyAsync(e => e.Id == id);
            if (isExist)
                return ServiceResponse<bool>.Ok(true, "Service type exist");
            return ServiceResponse<bool>.Error("Service type don't exist");
        }
    }
}
