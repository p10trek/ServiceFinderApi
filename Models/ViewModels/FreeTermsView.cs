using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.ViewModels
{
    public class FreeTermsView
    {
        public FreeTermsView()
        {
            FreeTermsBetween = new List<FreeTermBetween>();
        }
        public List<FreeTermBetween> FreeTermsBetween { get; set; }
        public DateTime FreeTermFrom { get; set; }
    }
    public class FreeTermBetween
    {
        public DateTime FreeTermStart { get; set; }
        public DateTime FreeTermEnd { get; set; }
    }
}
