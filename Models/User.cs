using System;
using System.Collections.Generic;

#nullable disable

namespace ServiceFinderApi.Models
{
    public partial class User
    {
        public User()
        {
            Orders = new HashSet<Order>();
            Providers = new HashSet<Provider>();
        }

        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsProvider { get; set; }
        public string Code { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Provider> Providers { get; set; }
    }
}
