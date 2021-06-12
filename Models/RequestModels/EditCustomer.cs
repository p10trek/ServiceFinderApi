using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.RequestModels
{
    public class EditCustomer
    {
        [Required]
        public string Login { get; set; }
        public string NewPassword { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
