using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceFinderApi.Models;
using ServiceFinderApi.Models.RequestModels;
using ServiceFinderApi.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        [HttpGet("/GetClientData")]
        public async Task<ServiceResponse<ClientView>> GetClientData(Guid orderId)
        {
            var result = await (from ord in _context.Orders.Where(r=>r.Id == orderId)
                                join cli in _context.Users on ord.CustomerId equals cli.Id
                                select new ClientView
                                {
                                    ClientName = cli.Name,
                                    ClientPhone = cli.Phone
                                }
                                ).FirstOrDefaultAsync();
            return ServiceResponse<ClientView>.Ok(result, "Order list successfully downloaded");
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
            {
                var FreeTerms = new List<FreeTermBetween>();
                FreeTerms.Add(new FreeTermBetween
                {
                    FreeTermStart = DateTime.Now,
                    FreeTermEnd = DateTime.Now.AddMonths(1),
                });
                return ServiceResponse<FreeTermsView>.Ok(new FreeTermsView
                {

                    FreeTermsBetween = FreeTerms,
                    FreeTermFrom = DateTime.Now
                }, "Free terms list successfully downloaded");
            }
            
            DateTime tmp_endTime = new DateTime(); //przechowuje date konca zamówienia poprzedniej iteracji
            bool isEven = false;
            var tmpS = new DateTime();
            var tmpE = new DateTime();
            var IsAtThisMoment = false;

    

            for(int n = 0; n<result.Count;n++)
            {
                if (n == 0)//obsluga pierwszego
                {
                    if (DateTime.Now < result[n].start) // jesli przed pierwsza usluga jest wolny czas
                    { 
                        tmpS = DateTime.Now;
                        tmpE = result[n].start;
                    }
                    else // jesli pierwsza usluga jest w trakcie
                    { 
                        tmpS = result[n].end;
                        tmpE = result[n+1]?.start?? DateTime.Now.AddMonths(1);
                        IsAtThisMoment = true;
                    }
                    freeTermBetween = new FreeTermBetween()
                    {
                        FreeTermStart = tmpS,
                        FreeTermEnd = tmpE - servDur
                    };
                    freeTerms.FreeTermsBetween.Add(freeTermBetween);
                    if (result.Last().id == result[n].id)
                        break;
                    else
                        continue;
                }
                if (n > 0) 
                {
                    if (IsAtThisMoment)
                    {
                        if (result.Count <= n +1) break;
                        tmpS = result[n].end;
                        tmpE = result[n + 1].start;
                    }
                    else
                    {
                        tmpS = result[n-1].end;
                        tmpE = result[n].start;
                    }
                    freeTermBetween = new FreeTermBetween()
                    {
                        FreeTermStart = tmpS,
                        FreeTermEnd = tmpE - servDur
                    };
                    freeTerms.FreeTermsBetween.Add(freeTermBetween);
                    //if (result.Last().id == result[n + 1]?.id)
                    //{
                    //    if (IsAtThisMoment)
                    //    {

                    //    }
                    //    else
                    //    {
                    //        tmpS = result[n].end;
                    //        tmpE = result[n + 1].start;
                    //        freeTermBetween = new FreeTermBetween()
                    //        {
                    //            FreeTermStart = tmpS,
                    //            FreeTermEnd = tmpE
                    //        };
                    //        freeTerms.FreeTermsBetween.Add(freeTermBetween);
                    //    }
                    //    break;
                    //}
                    continue;
                }
            }
            freeTerms.FreeTermsBetween.Add(new FreeTermBetween
            {
                FreeTermStart = result.Last().end,
                FreeTermEnd = DateTime.Now.AddMonths(1)
            });
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
        [Authorize]
        [AllowAnonymous]
        [HttpPut("/CreateOrder")]
        public async Task<ServiceResponse<bool>> Create(CreateOrder order)
        {
            if (!ModelState.IsValid)
                return ServiceResponse<bool>.Error("User adding error");
            
            string userLogin = User.Identity.Name ?? "Guest";

            var userId = await (from user in _context.Users
                                    .Where(row => row.Login == userLogin)
                                select user.Id).FirstOrDefaultAsync();

            var IsBusyTerm = await (from ord in _context.Orders
                         .Where(r => r.ProviderId == order.ProviderId)
                         .Where(r =>
                                (order.StartDate >= r.StartDate && order.StartDate <= r.EndTime)
                                ||
                                (order.EndDate >= r.StartDate && order.EndDate <= r.EndTime)
                                ||
                                (order.StartDate <= r.StartDate && order.EndDate >= r.EndTime)
                                )
                                select ord.Id).AnyAsync();

            if (IsBusyTerm)
            {
                return ServiceResponse<bool>.Error("Order can not be realised in this term");
            }

            if(order.ServiceId == Guid.Empty)
            {
                order.ServiceId = Guid.NewGuid();
                Service serviceToAdd = new Service
                {
                    Id = order.ServiceId,
                    ServiceName = order.ServiceName,
                    Description = order.ProviderComment,
                    Price = order.Duration,
                    ProviderId = order.ProviderId,
                    ServiceTypeId = await _context.ServiceTypes.Where(r=>r.TypeName == "Priced").Select(r=>r.Id).FirstOrDefaultAsync(),
                    Duration = order.Duration,
                    IsPrivate = true,

                };
                _context.Add(serviceToAdd);
            }

            Order orderToAdd = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = userId,
                CustomerComment = order.CustomerComment,
                EndTime = order.EndDate.AddSeconds(-1),
                ProviderId = order.ProviderId,
                
                Rate = order.Rate,
                ServiceId = order.ServiceId,
                StartDate = order.StartDate.AddSeconds(1),
                StatusId = await _context.ServiceStatuses
                .Where(row => row.Status == "Added")
                .Select(r=>r.Id).FirstOrDefaultAsync()
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


        [HttpGet("/MoveOrder")]
        public async Task<ServiceResponse<MoveOrderResponse>> Move(Guid orderId, DateTime newDate)
        {
            var order = await _context.Orders.Where(row => row.Id == orderId).FirstOrDefaultAsync();
            var notEmptys = await _context.Orders
                .Where(row => row.ProviderId == order.ProviderId)
                .Where(row => row.Id!=orderId)
                .Select(row => new
                {
                    from = row.StartDate,
                    to = row.EndTime
                }).ToListAsync();
            foreach(var notEmpty in notEmptys)
            {
                if(notEmpty.from<=newDate && notEmpty.to >= newDate)
                {
                    return ServiceResponse<MoveOrderResponse>.Error("Data allready reserved");
                }
            }

            var duration = order.EndTime - order.StartDate;
            var startMinutes = (newDate.Minute / 15)*15;
            var roundNewDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, newDate.Hour, startMinutes, 0);
            var newEndTime = roundNewDate + duration;
            order.StartDate = roundNewDate;
            order.EndTime = newEndTime;

            _context.Update(order).State = EntityState.Modified;
            _context.SaveChanges();
            var response = new MoveOrderResponse
            {
                provName = (await _context.Providers.Where(r=>r.Id == order.ProviderId).FirstOrDefaultAsync()).Name,
                clientName = (await _context.Users.Where(r=>r.Id == order.CustomerId).FirstOrDefaultAsync()).Login
            };

            return ServiceResponse<MoveOrderResponse>.Ok(response, "Data is not reserved");
            //todo dorobicc zapis do bazki
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
