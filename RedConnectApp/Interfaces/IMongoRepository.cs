using System.Linq.Expressions;
using MongoDB.Driver;
using RedConnect.Models;
using RedConnectApp.Enums;

namespace RedConnect.Interfaces
{




    public interface IMongoRepository
    {
        
        Task CreateUserAsync(MongoUser user);
        Task CreateBloodBankAsync(BloodBankDetails bloodBank);
        Task UpdateAsync(UpdateDefinition<MongoUser> update, FilterDefinition<MongoUser> filter);

        Task UpdateAsync(UpdateDefinition<BloodBankDetails> update, FilterDefinition<BloodBankDetails> filter);

        Task<MongoUser?> GetById(int id);
        Task<long> GetUserCount(Expression<Func<MongoUser, bool>> filter);
        Task<long> GetBloodBankCount();

        Task<List<MongoUser>> GetAllUsersAsync(Expression<Func<MongoUser, bool>> filter);

        Task<List<BloodBankDetails>> GetAllBloodBanksAsync();

        Task<BloodBankDetails?> GetBloodBankAsync(Expression<Func<BloodBankDetails, bool>> filter);
    }
}
