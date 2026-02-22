using MongoDB.Driver;
using RedConnect.Models;
using RedConnect.ViewModels;



public class DonorMapService
{
    private readonly IMongoCollection<RedConnect.Models.MongoUser> _collection;

    public DonorMapService(IConfiguration config)
    {
        var client = new MongoClient(config["Mongo:Connection"]);
        var db = client.GetDatabase(config["Mongo:Database"]);
        _collection = db.GetCollection<MongoUser>("Users");
    }

    public async Task<List<DonorMapViewModel>> GetActiveDonorsAsync()
    {
        var users = await _collection
            .Find(x => x.Active && x.AvailableLocation != null)
            .ToListAsync();

        return users
            .Where(x => x.AvailableLocation?.Coordinates?.Length == 2)
            .Select(x => new DonorMapViewModel
            {
                UserId = x.UserId,
                Lng = x.AvailableLocation.Coordinates[0],
                Lat = x.AvailableLocation.Coordinates[1],
                LocationText = x.LocationText,
                Name = x.UserDetails.Name,
                Phone = x.UserDetails.Phone,
                Concent= x.Concent,
                BloodGroup= x.BloodGroup,
            })
            .ToList();
    }
}