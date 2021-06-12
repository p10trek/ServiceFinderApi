using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.RequestModels
{
    public class CreateCustomer
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        public string Name { get; set; }
        [Required]
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
