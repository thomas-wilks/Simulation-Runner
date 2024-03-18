using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Simulate.FostersAndPartners.Shared.Models
{
    public class Result
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // Leaving as an opague object, but would tailor to expected results
        public Object Data { get; set; }
    }
}