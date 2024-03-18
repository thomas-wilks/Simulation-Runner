using MongoDB.Bson;

namespace Simulate.FostersAndPartners.Shared.Helpers
{
    public static class DatabaseHelpers
    {
        public static bool IsValidObjectId(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _);
        }
    }
}