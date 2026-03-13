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

public class MedicalReport
{
    public int    Index          { get; set; }
    public string Label          { get; set; }
    public string FilePath       { get; set; }
    public string Status         { get; set; } = "Pending"; // Pending | Approved | Rejected
    public string RejectedReason { get; set; }
}

public class UserDetails
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string NIC { get; set; }
    public string Phone { get; set; }
    public GenderEnum Gender { get; set; }
}

public class GeoLocation
{
    public string Type { get; set; } = "Point";
    public double[] Coordinates { get; set; }
}
