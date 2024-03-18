using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Simulate.FostersAndPartners.Shared.Models
{
    public class Simulation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string RequesterId { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; }
        public DateTime QueuedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}