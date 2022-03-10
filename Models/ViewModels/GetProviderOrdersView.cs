using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.ViewModels
{
    public class GetProviderOrdersView
    {
        public Guid id { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string color { get; set; }
	}
}
