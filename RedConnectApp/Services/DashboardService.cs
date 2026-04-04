using MongoDB.Driver;
using RedConnect.Exceptions;
using RedConnect.Interfaces;
using RedConnect.Models;

namespace RedConnect.Services
{
    public class DashboardService
    {
        private readonly IMongoRepository _mongoRepo;
        private readonly IAppDbContext _context;
        public DashboardService(IMongoRepository mongoRepo, IAppDbContext context) 
        {
            _mongoRepo = mongoRepo;
            _context = context;
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

            int id = 1;
            

            try
            {
                var _userCollection = await _mongoRepo.GetUserCount(f => f.Active == true);
                var totalDonors = await _mongoRepo.GetUserCount(f => f.Active == true && f.UserType == 0);
                var verifiedDonors = await _mongoRepo.GetUserCount(f => f.Verified == true);
                var totalBanks = await _mongoRepo.GetBloodBankCount();

                return (totalDonors, verifiedDonors, totalBanks);
            }
            catch (Exception)
            {
                throw new BusinessException("Error while Trying to login to the application");

            }
            
        }
    }
}
