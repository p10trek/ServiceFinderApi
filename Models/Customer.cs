using System;
using System.Collections.Generic;

#nullable disable

namespace ServiceFinderApi.Models
{
    public partial class Customer
    {
        public Customer()
        {
            Orders = new HashSet<Order>();
        }

        public Guid Id { get; set; }
        public string Login { get; set; }
        public byte[] Password { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
