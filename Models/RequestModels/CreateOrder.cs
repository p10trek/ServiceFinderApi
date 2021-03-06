using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.RequestModels
{
    public class CreateOrder
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProviderId { get; set; }
        public Guid ServiceId { get; set; }
        public string CustomerComment { get; set; }
        public string ProviderComment { get; set; }
        public decimal? Rate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid StatusId { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public string ServiceName { get; set; }
    }
}
