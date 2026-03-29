using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RedConnectApp.Enums;
using RedConnectApp.Models;

namespace RedConnect.Models;

public class MongoUser
{
    [BsonId]
    public ObjectId Id { get; set; }
    public int UserId { get; set; }
    public UserDetails UserDetails { get; set; }
    public GeoLocation DonatedLocation { get; set; }
    public GeoLocation AvailableLocation { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime LastUpdatedOn { get; set; }
    public int UserType { get; set; }

    public string LocationText { get; set; }

    public bool Concent { get; set; }

    public string BloodGroup { get; set; }
    public bool Verified { get; set; }
    public bool DocumentsUploaded { get; set; }
    public List<MedicalReport> MedicalReports { get; set; } = new();

    public List<DonateHistory> Donate_History { get; set; } = new();
}





