using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using RedConnect.DAL;
using RedConnect.Interfaces;
using RedConnect.Models;
using RedConnectApp.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


public class UserService : IUserService
{
    private readonly IMongoRepository _mongoRepo;
    private readonly IAppDbContext _context;

    public UserService(IMongoRepository mongoRepo, IAppDbContext context)
    {
        _mongoRepo = mongoRepo;
        _context = context;
    }

    public async Task RegisterAsync(int userTypeId, string email, string password,
           string name, string address, string nic,
           double donatedLng, double donatedLat,
           double availableLng, double availableLat, string locationSearch, string phone,
           GenderEnum gender, string bloodGroup)
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
                Phone = phone,
                Gender = gender
            },
            DonatedLocation = new GeoLocation
            {
                Coordinates = new[] { donatedLng, donatedLat }
            },
            AvailableLocation = new GeoLocation
            {
                Coordinates = new[] { availableLng, availableLat }
            },
            LocationText = locationSearch,
            BloodGroup = bloodGroup
        };
        await _mongoRepo.CreateUserAsync(mongoUser);

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
                Phone = phone,
                Gender = gender,
                Address = address ?? string.Empty,
                NIC = nic ?? string.Empty
            },
            DonatedLocation = new GeoLocation { Coordinates = new[] { 0.0, 0.0 } },
            AvailableLocation = new GeoLocation { Coordinates = new[] { availableLng, availableLat } },
            LocationText = locationText ?? string.Empty,
            BloodGroup = bloodGroup ?? string.Empty
        };
        await _mongoRepo.CreateUserAsync(mongoUser);
    }

    public async Task UpdateAsync(int userId, int userTypeId, string email, bool active, string name, string address,
        string nic, string phone, double donatedLng, double donatedLat, double availableLng, double availableLat,
        string locationText, bool concent, string bloodGroup)
    {
        var sqlUser = await _context.Users.FindAsync(userId);

        if (sqlUser == null)
            return;

        sqlUser.Email = email;
        sqlUser.UserTypeId = userTypeId;
        sqlUser.Active = active;
        sqlUser.LastUpdatedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();


        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);

        var update = Builders<MongoUser>.Update
            .Set(x => x.UserType, userTypeId)
            .Set(x => x.Active, active)
            .Set(x => x.LastUpdatedOn, DateTime.UtcNow)
            .Set(x => x.UserDetails, new UserDetails
            {
                Name = name,
                Address = address,
                NIC = nic,
                Phone = phone
            })
            .Set(x => x.LocationText, locationText)
            .Set(x => x.Concent, concent)
            .Set(x => x.BloodGroup, bloodGroup)
            .Set(x => x.DonatedLocation, new GeoLocation { Coordinates = new[] { donatedLng, donatedLat } })
            .Set(x => x.AvailableLocation, new GeoLocation { Coordinates = new[] { availableLng, availableLat } })
            .SetOnInsert(x => x.CreatedOn, DateTime.UtcNow);

        await _mongoRepo.UpdateAsync(update, filter);
    }

    public async Task<MsSqlUser?> LoginAsync(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == email && x.Active);

        if (user == null)
            return null;

        return BCrypt.Net.BCrypt.Verify(password, user.Password) ? user : null;
    }

    public async Task<List<UserType>> GetUserTypes()
    {
        var userTypes = await _context.UserType.ToListAsync();
        return userTypes;
    }

    public IQueryable<UserType> GetUserTypes(bool Queryable)
    {
        return _context.UserType;
    }

    public async Task<Dictionary<int, string>> GetUserTypeDictionaryAsync()
    {
        return await _context.UserType
            .ToDictionaryAsync(x => x.UserTypeId, x => x.UserTypeName);
    }

    public async Task<MongoUser?> GetUserById(int id)
    {
        return await _mongoRepo.GetById(id);
    }

    public async Task<MsSqlUser?> GetUserById(int id, bool IsSql)
    {
        return await _context.Users.FirstOrDefaultAsync(f => f.UserId == id);
    }

    public async Task VerifyDonorAsync(int userId)
    {
        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);

        var update = Builders<MongoUser>.Update
            .Set(x => x.Verified, true)
            .Set(x => x.LastUpdatedOn, DateTime.UtcNow);

        await _mongoRepo.UpdateAsync(update, filter);
    }

    public async Task<List<MongoUser>> GetAllDonorsAsync()
    {
        return await _mongoRepo.GetAllUsersAsync(x => x.UserType == 0);
    }

    public async Task<List<MongoUser>> GetAllUsersAsync(bool IsActive, bool IsVerified)
    {
        return await _mongoRepo.GetAllUsersAsync(x => x.Active == IsActive && x.Verified == IsVerified);
    }

    public async Task<List<MongoUser>> GetAllUsersAsync(bool IsActive, int userType)
    {
        return await _mongoRepo.GetAllUsersAsync(x => x.Active == IsActive && x.UserType == userType);
    }

    public async Task<List<MsSqlUser>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<List<MongoUser>> GetAllUsersAsync(bool IsMongo = true)
    {
        return await _mongoRepo.GetAllUsersAsync(_=> true);
    }

    public async Task<bool> VerifyPasswordAsync(int userId, string password)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        return BCrypt.Net.BCrypt.Verify(password, user.Password);
    }


    public async Task ChangePasswordAsync(int userId, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.LastUpdatedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync();
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
        await _mongoRepo.UpdateAsync( update, filter);
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
        await _mongoRepo.UpdateAsync(update, filter);
    }

    public async Task<List<BloodBankDetails>> GetAllBloodBanksAsync()
    {
        return await _mongoRepo.GetAllBloodBanksAsync();
    }

    public async Task<List<UserType>> GetAllUserTypesAsync()
    {
        return await _context.UserType.ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email);
    }

    public async Task Donate(int userId, object donation)
    {
        var update = Builders<MongoUser>.Update.Push(x => x.Donate_History, donation);
        var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);
        await _mongoRepo.UpdateAsync(update, filter);
    }

}