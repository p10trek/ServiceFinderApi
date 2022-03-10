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
    public class OrderController : MyControllerBase
    {
        private readonly ServiceFinderDBContext _context;
        public OrderController(ServiceFinderDBContext context)
        {
            _context = context;
        }

        [HttpGet("/GetProviderOrders")]
        public async Task<ServiceResponse<List<GetProviderOrdersView>>> Index(Guid providerId)
        {
            var result = await (from ord in _context.Orders.Where(row => row.ProviderId == providerId)
                                join serv in _context.Services on ord.ServiceId equals serv.Id
                                select new GetProviderOrdersView
                                {
                                    id = ord.Id,
                                    title = serv.ServiceName,
                                    start = ord.StartDate,
                                    end = ord.EndTime,
                                }).ToListAsync();

            return ServiceResponse<List<GetProviderOrdersView>>.Ok(result, "Order list successfully downloaded");
        }
        [HttpGet("/GetFreeTerms")]
        public async Task<ServiceResponse<FreeTermsView>> GetFreeTerms(Guid providerId, int serviceDuration)
        {
            int counter = 0;
            FreeTermsView freeTerms = new FreeTermsView();
            TimeSpan servDur = new TimeSpan(serviceDuration, 0, 0);
            var result = await (from ord in _context.Orders.Where(row => row.ProviderId == providerId)
                                join serv in _context.Services on ord.ServiceId equals serv.Id
                                where ord.EndTime >= DateTime.Now
                                select new GetProviderOrdersView
                                {
                                    id = ord.Id,
                                    start = ord.StartDate,
                                    end = ord.EndTime,
                                }).OrderBy(r=>r.start)
                                .ToListAsync();
            FreeTermBetween freeTermBetween = new FreeTermBetween();
            if (result.Count == 0)
                return ServiceResponse<FreeTermsView>.Ok(new FreeTermsView
                {

                    FreeTermsBetween = new List<FreeTermBetween>(),
                    FreeTermFrom = DateTime.Now
                }, "Free terms list successfully downloaded");
            foreach (var el in result)
            {

                if (counter == 0) 
                {
                    freeTermBetween = new FreeTermBetween();
                    freeTermBetween.FreeTermStart = el.end;
                    if (result.Last().id == el.id)
                    {
                        freeTermBetween.FreeTermEnd = DateTime.Now.AddMonths(1);
                        freeTerms.FreeTermsBetween.Add(freeTermBetween);
                        counter = 0;
                        continue;
                    }
                    counter++;
                }
                else
                { 
                    freeTermBetween.FreeTermEnd = el.start-servDur;
                    TimeSpan timeB = freeTermBetween.FreeTermEnd - freeTermBetween.FreeTermStart;
                    if (timeB>=servDur)
                    freeTerms.FreeTermsBetween.Add(freeTermBetween);
                    counter = 0;
                }
            }
            freeTerms.FreeTermFrom = result.Last().end;
            return ServiceResponse<FreeTermsView>.Ok(freeTerms, "Free terms list successfully downloaded");
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
                EndTime = order.EndDate,
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
                order.EndTime = editedOrder.EndDate;
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
