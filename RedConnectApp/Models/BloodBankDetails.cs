using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RedConnect.Models;
public class BloodBankDetails
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string LocationName { get; set; }
    public string Address { get; set; }
    public string LocationText { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }

    public List<int> UserIds { get; set; } = new();

    public DateTime CreatedOn { get; set; }
}
