using RedConnect.Models;

namespace RedConnect.Interfaces
{
    public interface IBloodBankService
    {
        Task CreateOrUpdateBloodBankAsync(
        string locationName,
        string address,
        string email,
        string password,
        int userTypeId,
        double lat = 0, double lng = 0, string locationText = "");

        Task<BloodBankDetails?> GetBloodBankByLocationAsync(string locationName);

        Task<long> GetBloodBankCount();
    }
}
