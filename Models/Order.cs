using System;
using System.Collections.Generic;

#nullable disable

namespace ServiceFinderApi.Models
{
    public partial class Order
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProviderId { get; set; }
        public Guid ServiceId { get; set; }
        public string CustomerComment { get; set; }
        public string ProviderComment { get; set; }
        public decimal? Rate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }
        public Guid StatusId { get; set; }

        public virtual User Customer { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Service Service { get; set; }
        public virtual ServiceStatus Status { get; set; }
    }
}
