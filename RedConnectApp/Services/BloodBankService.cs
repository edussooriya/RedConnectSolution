using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RedConnect.Exceptions;
using RedConnect.Interfaces;
using RedConnect.Models;

namespace RedConnect.Services
{
    public class BloodBankService:IBloodBankService
    {
        private readonly IMongoRepository _mongoRepo;
        private readonly IAppDbContext _context;

        public BloodBankService(IMongoRepository mongoRepo, IAppDbContext context) 
        { 
            _mongoRepo = mongoRepo;
            _context = context;
        }

        public async Task CreateOrUpdateBloodBankAsync(
        string locationName,
        string address,
        string email,
        string password,
        int userTypeId,
        double lat = 0, double lng = 0, string locationText = "")
        {
            //Check if email exists in SQL
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
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw new BusinessException("Error while Trying to Create a blood bank User");
                }
               
            }
            else
            {
                user = existingUser;
            }

            // Check if BloodBank location exists
            var existingBank = await GetBloodBankByLocationAsync(locationName);

            if (existingBank == null)
            {
                // Create new BloodBank document
                var bloodBank = new BloodBankDetails
                {
                    LocationName = locationName,
                    Address = address,
                    LocationText = locationText ?? string.Empty,
                    Lat = lat,
                    Lng = lng,
                    UserIds = new List<int> { user.UserId },
                    CreatedOn = DateTime.UtcNow
                };
                try
                {
                    await _mongoRepo.CreateBloodBankAsync(bloodBank);
                }
                catch (Exception)
                {

                    throw new BusinessException("Error while Trying to Create a blood bank");
                }
                
            }
            else
            {
                //Add userId only if not already inside array
                if (!existingBank.UserIds.Contains(user.UserId))
                {
                    var filter = Builders<BloodBankDetails>.Filter
                        .Eq(x => x.LocationName, locationName);

                    var update = Builders<BloodBankDetails>.Update
                        .AddToSet(x => x.UserIds, user.UserId);
                    try
                    {
                        await _mongoRepo.UpdateAsync(update, filter);
                    }
                    catch (Exception)
                    {

                        throw new BusinessException("Error while Trying to Update blood bank details");
                    }
                    
                }
            }
        }

        public async Task<BloodBankDetails?> GetBloodBankByLocationAsync(string locationName)
        {
            return await _mongoRepo.GetBloodBankAsync(f => f.LocationName == locationName);
        }

        public async Task<long> GetBloodBankCount()
        {
            return await _mongoRepo.GetBloodBankCount();
        }
    }
}
