using BCrypt.Net;
using MongoDB.Driver;
using RedConnect.Models;
using RedConnect.Data;

namespace RedConnect.DAL;

public class UserRepository
{
    private readonly AppDbContext _context;
    private readonly IMongoCollection<MongoUser> _mongoCollection;

    public UserRepository(AppDbContext context, IConfiguration config)
    {
        _context = context;
        var client = new MongoClient(config["Mongo:Connection"]);
        var db = client.GetDatabase(config["Mongo:Database"]);
        _mongoCollection = db.GetCollection<MongoUser>("Users");
    }

    public async Task RegisterAsync(int userTypeId, string email, string password,
        string name, string address, string nic,
        double donatedLng, double donatedLat,
        double availableLng, double availableLat,string locationSearch,string phone)
    {
        var hashed = BCrypt.Net.BCrypt.HashPassword(password);

        var sqlUser = new MsSqlUser
        {
            UserTypeId = userTypeId,
            Email = email,
            Password = hashed,
            Active = true
        };

        _context.Users.Add(sqlUser);
        await _context.SaveChangesAsync();

        var mongoUser = new MongoUser
        {
            UserId = sqlUser.UserId,
            UserType = userTypeId,
            Active = true,
            CreatedOn = DateTime.UtcNow,
            LastUpdatedOn = DateTime.UtcNow,
            UserDetails = new UserDetails
            {
                Name = name,
                Address = address,
                NIC = nic,
                Phone = phone
            },
            DonatedLocation = new GeoLocation
            {
                Coordinates = new[] { donatedLng, donatedLat }
            },
            AvailableLocation = new GeoLocation
            {
                Coordinates = new[] { availableLng, availableLat }
            },

            LocationText = locationSearch
        };

        await _mongoCollection.InsertOneAsync(mongoUser);
    }

    public async Task<MsSqlUser?> LoginAsync(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == email && x.Active);

        if (user == null)
            return null;

        return BCrypt.Net.BCrypt.Verify(password, user.Password) ? user : null;
    }



    public async Task UpdateAsync(int userId,
    int userTypeId,
    string email,
    bool active,
    string name,
    string address,
    string nic,
    string phone,
    double donatedLng,
    double donatedLat,
    double availableLng,
    double availableLat,string locationText, bool concent, string bloodBroup)
    {
        // 🔹 Update MSSQL
        var sqlUser = await _context.Users.FindAsync(userId);

        if (sqlUser == null)
            return;

        sqlUser.Email = email;
        sqlUser.UserTypeId = userTypeId;
        sqlUser.Active = active;
        sqlUser.LastUpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 🔹 Update MongoDB
        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);

        var update = Builders<MongoUser>.Update
            .Set(x => x.UserType, userTypeId)
            .Set(x => x.Active, active)
            .Set(x => x.LastUpdatedOn, DateTime.UtcNow)
            .Set(x => x.UserDetails.Name, name)
            .Set(x => x.UserDetails.Address, address)
            .Set(x => x.UserDetails.NIC, nic)
            .Set(x => x.UserDetails.Phone, phone)
            .Set(x => x.LocationText, locationText)
            .Set(x => x.Concent, concent)
            .Set(x => x.BloodGroup, bloodBroup)
            .Set(x => x.DonatedLocation.Coordinates, new[] { donatedLng, donatedLat })
            .Set(x => x.AvailableLocation.Coordinates, new[] { availableLng, availableLat });

        await _mongoCollection.UpdateOneAsync(filter, update);
    }

    public async Task<MsSqlUser?> GetByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<MongoUser?> GetMongoUserAsync(int userId)
    {
        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);
        return await _mongoCollection.Find(filter).FirstOrDefaultAsync();
    }


}
