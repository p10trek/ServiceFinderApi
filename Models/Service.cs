using System;
using System.Collections.Generic;

#nullable disable

namespace ServiceFinderApi.Models
{
    public partial class Service
    {
        public Service()
        {
            Orders = new HashSet<Order>();
        }

        public Guid Id { get; set; }
        public string ServiceName { get; set; }
        public Guid ProviderId { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public Guid ServiceTypeId { get; set; }
        public decimal Duration { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsArchival { get; set; }

        public virtual Provider Provider { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
