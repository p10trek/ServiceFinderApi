using System;
using System.Collections.Generic;

#nullable disable

namespace ServiceFinderApi.Models
{
    public partial class ServiceType
    {
        public ServiceType()
        {
            Orders = new HashSet<Order>();
        }

        public Guid Id { get; set; }
        public string TypeName { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
