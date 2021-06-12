using System;
using System.Collections.Generic;

#nullable disable

namespace ServiceFinderApi.Models
{
    public partial class Provider
    {
        public Provider()
        {
            Orders = new HashSet<Order>();
            Services = new HashSet<Service>();
        }

        public Guid Id { get; set; }
        public string Login { get; set; }
        public byte[] Password { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Logo { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string PostalCode { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}
