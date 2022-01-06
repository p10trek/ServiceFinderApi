﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFinderApi.Models.ViewModels
{
    public class ServiceView
    {
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ServiceType { get; set; }
    }
}
