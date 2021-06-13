using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.RequestModels
{
    public class CreateService
    {
        public string ServiceName { get; set; }
        public Guid ProviderId { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
        public Guid ServiceTypeId { get; set; }
    }
}
