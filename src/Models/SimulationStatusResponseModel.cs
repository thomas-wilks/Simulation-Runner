using System;

namespace Simulate.FostersAndPartners.Service.Models
{
    public class SimulationStatusResponseModel
    {
        public string Status { get; set; }
        public DateTime QueuedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}