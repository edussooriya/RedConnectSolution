using System.Collections;
using System.Linq.Expressions;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using RedConnect.Interfaces;
using RedConnect.Models;
using RedConnectApp.DAL;
using RedConnectApp.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RedConnect.DAL;

public class MongoRepository : IMongoRepository
{
    private readonly IAppDbContext _context;
    private readonly IMongoCollection<MongoUser> _userCollection;
    private readonly IMongoCollection<BloodBankDetails> _bloodBankCollection;
    private readonly IMongoDatabase _db;

    public MongoRepository(MSSQLDBContext context, IConfiguration config)
    {
        _context = context;
        var client = new MongoClient(config["Mongo:Connection"]);
        _db = client.GetDatabase(config["Mongo:Database"]);
        _userCollection      = _db.GetCollection<MongoUser>("Users");
        _bloodBankCollection  = _db.GetCollection<BloodBankDetails>("BloodBankDetails");
    }

    /// <summary>
    /// One-time migration: converts MedicalReports from List&lt;string&gt; to List&lt;MedicalReport&gt;
    /// using raw BsonDocument access so deserialization of the old format never fails.
    /// </summary>
    public async Task MigrateMedicalReportsAsync()
    {
        var raw = _db.GetCollection<BsonDocument>("Users");
        var labels = new[] { "Blood Test Report", "Medical History", "Doctor's Certificate" };

        var docs = await raw.Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();

        foreach (var doc in docs)
        {
            if (!doc.Contains("MedicalReports")) continue;

            var reportsField = doc["MedicalReports"];
            if (reportsField.BsonType != BsonType.Array) continue;

            var arr = reportsField.AsBsonArray;
            if (arr.Count == 0) continue;

            // Already migrated if first element is a document
            if (arr[0].BsonType == BsonType.Document) continue;

            // Convert each string path to a MedicalReport document
            var newArr = new BsonArray();
            for (int i = 0; i < arr.Count; i++)
            {
                var fp = arr[i].BsonType == BsonType.String ? arr[i].AsString : string.Empty;
                newArr.Add(new BsonDocument
                {
                    { "Index",          i },
                    { "Label",          i < labels.Length ? labels[i] : $"Document {i + 1}" },
                    { "FilePath",       fp },
                    { "Status",         "Pending" },
                    { "RejectedReason", BsonNull.Value }
                });
            }

            var filter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
            var update = Builders<BsonDocument>.Update.Set("MedicalReports", newArr);
            await raw.UpdateOneAsync(filter, update);
        }
    }

    public async Task CreateUserAsync(MongoUser mongoUser)
    {
        await _userCollection.InsertOneAsync(mongoUser);
    }

    public async Task CreateBloodBankAsync(BloodBankDetails bloodBank)
    {
        await _bloodBankCollection.InsertOneAsync(bloodBank);
    }

    public async Task UpdateAsync(UpdateDefinition<MongoUser> update, FilterDefinition<MongoUser> filter)
    {
        await _userCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task UpdateAsync(UpdateDefinition<BloodBankDetails> update, FilterDefinition<BloodBankDetails> filter)
    {
        await _bloodBankCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task<MongoUser?> GetMongoUserAsync(int userId)
    {
        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);
        return await _userCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<long> GetUserCount(Expression<Func<MongoUser, bool>> filter)
    {
        return await _userCollection.CountDocumentsAsync(filter);
    }

    public async Task<long> GetBloodBankCount()
    {
        return await _bloodBankCollection.CountDocumentsAsync(_ => true);
    }

    public async Task<List<MongoUser>> GetUnverifiedDonorsAsync()
    {
        var filter = Builders<MongoUser>.Filter.And(
             Builders<MongoUser>.Filter.Eq(x => x.UserType, 0),
             Builders<MongoUser>.Filter.Or(
                 Builders<MongoUser>.Filter.Eq(x => x.Verified, false),
                 Builders<MongoUser>.Filter.Exists("Verified", false)
        ));

        var users = await _userCollection.Find(filter).ToListAsync();
        return users;
    }

    

    public async Task VerifyDonorAsync(int userId)
    {
        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);

        var update = Builders<MongoUser>.Update
            .Set(x => x.Verified, true)
            .Set(x => x.LastUpdatedOn, DateTime.UtcNow);

        await _userCollection.UpdateOneAsync(filter, update);
    }

    public async Task CreateBloodBankAsync(
    string locationName,
    string address,
    string email,
    string password,
    int userTypeId)
    {
        // 🔹 1. Create User in MSSQL
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new MsSqlUser
        {
            Email = email,
            Password = hashed,
            UserTypeId = userTypeId,
            Active = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 🔹 2. Create BloodBank document in Mongo
        var bloodBank = new BloodBankDetails
        {
            LocationName = locationName,
            Address = address,
            UserIds = new List<int> { user.UserId },
            CreatedOn = DateTime.UtcNow
        };

        await _bloodBankCollection.InsertOneAsync(bloodBank);
    }

    public async Task<BloodBankDetails?> GetBloodBankByLocationAsync(string locationName)
    {
        var filter = Builders<BloodBankDetails>.Filter
            .Eq(x => x.LocationName, locationName);

        return await _bloodBankCollection
            .Find(filter)
            .FirstOrDefaultAsync();
    }

    public async Task<BloodBankDetails?> GetBloodBankAsync(Expression<Func<BloodBankDetails, bool>> filter)
    {
        return await _bloodBankCollection
            .Find(filter)
            .FirstOrDefaultAsync();
    }



    public async Task CreateOrUpdateBloodBankAsync(
    string locationName,
    string address,
    string email,
    string password,
    int userTypeId,
    double lat = 0, double lng = 0, string locationText = "")
    {
        // 🔹 1️⃣ Check if email exists in SQL
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        MsSqlUser user;

        if (existingUser == null)
        {
            // Create new user
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);

            user = new MsSqlUser
            {
                Email = email,
                Password = hashed,
                UserTypeId = userTypeId,
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            user = existingUser;
        }

        // 🔹 2️⃣ Check if BloodBank location exists
        var existingBank = await GetBloodBankByLocationAsync(locationName);

        if (existingBank == null)
        {
            // Create new BloodBank document
            var bloodBank = new BloodBankDetails
            {
                LocationName = locationName,
                Address      = address,
                LocationText = locationText ?? string.Empty,
                Lat          = lat,
                Lng          = lng,
                UserIds      = new List<int> { user.UserId },
                CreatedOn    = DateTime.UtcNow
            };

            await _bloodBankCollection.InsertOneAsync(bloodBank);
        }
        else
        {
            // 🔹 Add userId only if not already inside array
            if (!existingBank.UserIds.Contains(user.UserId))
            {
                var filter = Builders<BloodBankDetails>.Filter
                    .Eq(x => x.LocationName, locationName);

                var update = Builders<BloodBankDetails>.Update
                    .AddToSet(x => x.UserIds, user.UserId);

                await _bloodBankCollection.UpdateOneAsync(filter, update);
            }
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task<List<MongoUser>> GetAllUsersAsync(Expression<Func<MongoUser, bool>> filter)
    {
        //var filter = Builders<MongoUser>.Filter.Eq(x => x.UserType, 0);
        return await _userCollection.Find(filter).ToListAsync();
    }

    public async Task<List<BloodBankDetails>> GetAllBloodBanksAsync()
    {
        return await _bloodBankCollection
            .Find(FilterDefinition<BloodBankDetails>.Empty)
            .ToListAsync();
    }

    public async Task DeactivateUserAsync(int userId)
    {
        var sqlUser = await _context.Users.FindAsync(userId);
        if (sqlUser != null)
        {
            sqlUser.Active = false;
            sqlUser.LastUpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);
        var update = Builders<MongoUser>.Update
            .Set(x => x.Active, false)
            .Set(x => x.LastUpdatedOn, DateTime.UtcNow);
        await _userCollection.UpdateOneAsync(filter, update);
    }

    

    

    public async Task<List<MongoUser>> GetAllMongoUsersAsync()
    {
        return await _userCollection
            .Find(FilterDefinition<MongoUser>.Empty)
            .ToListAsync();
    }

    public async Task<MongoUser?> GetById(int id)
    {
        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, id);
        return await _userCollection.Find(filter).FirstOrDefaultAsync();
            
    }

    public async Task ReactivateUserAsync(int userId)
    {
        var sqlUser = await _context.Users.FindAsync(userId);
        if (sqlUser != null)
        {
            sqlUser.Active = true;
            sqlUser.LastUpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);
        var update = Builders<MongoUser>.Update
            .Set(x => x.Active, true)
            .Set(x => x.LastUpdatedOn, DateTime.UtcNow);
        await _userCollection.UpdateOneAsync(filter, update);
    }

    public async Task AdminCreateUserAsync(
        int userTypeId, string email, string password,
        string name, string phone, GenderEnum gender, string bloodGroup,
        string address = "", string locationText = "",
        double availableLat = 0, double availableLng = 0,
        string nic = "")
    {
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);

        var sqlUser = new MsSqlUser
        {
            UserTypeId = userTypeId,
            Email      = email,
            Password   = hashed,
            Active     = true
        };
        _context.Users.Add(sqlUser);
        await _context.SaveChangesAsync();

        var mongoUser = new MongoUser
        {
            UserId        = sqlUser.UserId,
            UserType      = userTypeId,
            Active        = true,
            CreatedOn     = DateTime.UtcNow,
            LastUpdatedOn = DateTime.UtcNow,
            UserDetails   = new UserDetails
            {
                Name    = name,
                Phone   = phone,
                Gender  = gender,
                Address = address      ?? string.Empty,
                NIC     = nic          ?? string.Empty
            },
            DonatedLocation   = new GeoLocation { Coordinates = new[] { 0.0, 0.0 } },
            AvailableLocation = new GeoLocation { Coordinates = new[] { availableLng, availableLat } },
            LocationText      = locationText ?? string.Empty,
            BloodGroup        = bloodGroup  ?? string.Empty
        };
        await _userCollection.InsertOneAsync(mongoUser);
    }

    public async Task<(long TotalDonors, long VerifiedDonors, long TotalBanks)> GetDashboardStatsAsync()
    {
        var donorFilter = Builders<MongoUser>.Filter.And(
            Builders<MongoUser>.Filter.Eq(x => x.UserType, 0),
            Builders<MongoUser>.Filter.Eq(x => x.Active, true)
        );
        var verifiedFilter = Builders<MongoUser>.Filter.And(
            donorFilter,
            Builders<MongoUser>.Filter.Eq(x => x.Verified, true)
        );

        var totalDonors    = await _userCollection.CountDocumentsAsync(donorFilter);
        var verifiedDonors = await _userCollection.CountDocumentsAsync(verifiedFilter);
        var totalBanks     = await _bloodBankCollection.CountDocumentsAsync(FilterDefinition<BloodBankDetails>.Empty);

        return (totalDonors, verifiedDonors, totalBanks);
    }

    public async Task Donate(int userId, object donation)
    {
        var update = Builders<MongoUser>.Update.Push(x => x.Donate_History, donation);

        await _userCollection.UpdateOneAsync(x => x.UserId == userId, update);
    }
}
