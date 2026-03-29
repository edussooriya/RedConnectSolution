using RedConnect.Models;
using RedConnectApp.Enums;

namespace RedConnect.Interfaces
{
    public interface IUserService
    {
        Task RegisterAsync(int userTypeId, string email, string password,
           string name, string address, string nic,
           double donatedLng, double donatedLat,
           double availableLng, double availableLat, string locationSearch, string phone,
           GenderEnum gender, string bloodGroup);
        Task UpdateAsync(int userId, int userTypeId, string email, bool active, string name, string address,
        string nic, string phone, double donatedLng, double donatedLat, double availableLng, double availableLat,
        string locationText, bool concent, string bloodGroup);

        Task AdminCreateUserAsync(
        int userTypeId, string email, string password,
        string name, string phone, GenderEnum gender, string bloodGroup,
        string address = "", string locationText = "",
        double availableLat = 0, double availableLng = 0,
        string nic = "");

        Task<MsSqlUser?> LoginAsync(string email, string password);

        Task<MongoUser?> GetUserById(int id);

        Task<MsSqlUser?> GetUserById(int id,bool IsSql);

        Task VerifyDonorAsync(int userId);

        Task<List<MongoUser>> GetAllDonorsAsync();

        Task<List<BloodBankDetails>> GetAllBloodBanksAsync();

        Task DeactivateUserAsync(int userId);

        Task ReactivateUserAsync(int userId);

        Task<List<UserType>> GetAllUserTypesAsync();

        Task<bool> EmailExistsAsync(string email);

        Task<List<MongoUser>> GetAllUsersAsync(bool IsActive, bool IsVerified);

        Task<List<MongoUser>> GetAllUsersAsync(bool IsActive, int userType);

        Task<bool> VerifyPasswordAsync(int userId, string password);

        Task ChangePasswordAsync(int userId, string newPassword);

        Task<List<MsSqlUser>> GetAllUsersAsync();

        Task<List<MongoUser>> GetAllUsersAsync(bool IsMongo = true);

        Task Donate(int userId, object donation);
    }
}

