using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.RequestModels
{
    public class EditOrder
    {
        public Guid Id { get; set; }
        public string CustomerComment { get; set; }
        public string ProviderComment { get; set; }
        public decimal? Rate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid StatusId { get; set; }
    }
}
