using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.RequestModels
{
    public class CreateService
    {
        [Required]
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public bool ServiceType { get; set; }
        public int Duration { get; set; }
    }
}
