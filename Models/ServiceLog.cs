using System;
using System.Collections.Generic;

#nullable disable

namespace ServiceFinderApi.Models
{
    public partial class ServiceLog
    {
        public Guid Id { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid ServicesStatusId { get; set; }
        public string ProviderMessage { get; set; }
    }
}
