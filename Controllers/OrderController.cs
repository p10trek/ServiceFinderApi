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
    public class OrderController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;
        public OrderController(ServiceFinderDBContext context)
        {
            _context = context;
        }

        [HttpGet("/GetOrders")]
        public async Task<ServiceResponse<List<Order>>> Index()
        {
            return ServiceResponse<List<Order>>.Ok(await _context.Orders.ToListAsync(), "Order list successfully downloaded");
        }

        [HttpPost("/GetOrder")]
        public async Task<ServiceResponse<Order>> Details(Guid? id)
        {
            if (id == null)
            {
                return ServiceResponse<Order>.Error("Id parameter is null");
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return ServiceResponse<Order>.Error("Id not match to any order");
            }

            return ServiceResponse<Order>.Ok(order, "Order found");
        }

        [HttpPut("/CreateOrder")]
        public async Task<ServiceResponse<bool>> Create(CreateOrder order)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");

            Order orderToAdd = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = order.CustomerId,
                CustomerComment = order.CustomerComment,
                EndDate = order.EndDate,
                ProviderId = order.ProviderId,
                ProviderComment = order.ProviderComment,
                Rate = order.Rate,
                ServiceId = order.ServiceId,
                StartDate = order.StartDate,
                StatusId = order.StatusId
            };

            _context.Add(orderToAdd);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "Order was added");
        }

        [HttpPost("/EditOrder")]
        public async Task<ServiceResponse<bool>> Edit(EditOrder editedOrder)
        {
            var order = await _context.Orders
                                        .Where(row => row.Id == editedOrder.Id)
                                        .FirstOrDefaultAsync();
            if (order == null)
            {
                return ServiceResponse<bool>.Error("Order not Found");
            }
            if (!String.IsNullOrEmpty(editedOrder.CustomerComment))
                order.CustomerComment = editedOrder.CustomerComment;
            if (editedOrder.EndDate != DateTime.MinValue)
                order.EndDate = editedOrder.EndDate;
            if (!String.IsNullOrEmpty(editedOrder.ProviderComment))
                order.ProviderComment = editedOrder.ProviderComment;
            if (editedOrder.Rate != 0)
                order.Rate = editedOrder.Rate;
            if (editedOrder.StartDate != DateTime.MinValue)
                order.StartDate = editedOrder.StartDate;
            if (editedOrder.StatusId != Guid.Empty)
                order.StatusId = editedOrder.StatusId;

            _context.Update(order);
            await _context.SaveChangesAsync();
            return ServiceResponse<bool>.Ok(true, "Order edited corectly");
        }

        [HttpDelete("/DeleteOrder")]
        public async Task<ServiceResponse<bool>> Delete(Guid OrderId)
        {
            var order = await _context.Orders.Where(row => row.Id == OrderId).FirstOrDefaultAsync();
            if (order == null)
            {
                return ServiceResponse<bool>.Error("Order not found");
            }

            _context.Remove(order);
            await _context.SaveChangesAsync();

            return ServiceResponse<bool>.Ok(true, "Order succesfully deleted");
        }

        [HttpGet("/IfOrderExist")]
        public async Task<ServiceResponse<bool>> OrderExists(Guid id)
        {
            bool isExist = await _context.Orders.AnyAsync(e => e.Id == id);
            if (isExist)
                return ServiceResponse<bool>.Ok(true, "Order exist");
            return ServiceResponse<bool>.Error("Order don't exist");
        }
    }
}
