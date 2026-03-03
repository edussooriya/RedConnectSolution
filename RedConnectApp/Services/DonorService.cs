using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RedConnect.Models;
using RedConnect.ViewModels;

public class DonorMapService
{
    private readonly IMongoCollection<MongoUser> _collection;

    public DonorMapService(IConfiguration config)
    {
        var client = new MongoClient(config["Mongo:Connection"]);
        var db = client.GetDatabase(config["Mongo:Database"]);
        _collection = db.GetCollection<MongoUser>("Users");
    }

    public async Task<List<DonorMapViewModel>> GetActiveDonorsAsync(string bloodGroup = null)
    {
        var users = await _collection
            .Find(x => x.Active && x.AvailableLocation != null)
            .ToListAsync();

        var query = users.Where(x => x.AvailableLocation?.Coordinates?.Length == 2);

        if (!string.IsNullOrEmpty(bloodGroup))
            query = query.Where(x => x.BloodGroup == bloodGroup);

        return query.Select(x => new DonorMapViewModel
        {
            UserId    = x.UserId,
            Lng       = x.AvailableLocation.Coordinates[0],
            Lat       = x.AvailableLocation.Coordinates[1],
            LocationText = x.LocationText,
            Name      = x.UserDetails.Name,
            Phone     = x.UserDetails.Phone,
            Concent   = x.Concent,
            BloodGroup = x.BloodGroup.IsNullOrEmpty() ? "N/A" : x.BloodGroup,
            Verified  = x.Verified,
        }).ToList();
    }
}
