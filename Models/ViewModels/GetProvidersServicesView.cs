﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.ViewModels
{
    public class GetProvidersServicesView
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; }
        public Guid ProviderId { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }
        public bool IsPriced { get; set; }
    }
}
