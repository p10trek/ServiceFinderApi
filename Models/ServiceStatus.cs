using System;
using System.Collections.Generic;

#nullable disable

namespace ServiceFinderApi.Models
{
    public partial class ServiceStatus
    {
        public ServiceStatus()
        {
            Orders = new HashSet<Order>();
        }

        public Guid Id { get; set; }
        public string Status { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
