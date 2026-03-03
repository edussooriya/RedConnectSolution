using Microsoft.EntityFrameworkCore;
using RedConnect.DAL;
using RedConnect.Models;
using RedConnectApp.DAL;
using RedConnectApp.Enums;

namespace RedConnectApp.Services;

public class DataSeeder
{
    private readonly MSSQLDBContext _sql;
    private readonly MongoRepository _repo;

    public DataSeeder(MSSQLDBContext sql, MongoRepository repo)
    {
        _sql  = sql;
        _repo = repo;
    }

    public async Task SeedAsync()
    {
        // ── 0. Migrate MedicalReports field (string[] → MedicalReport[]) ───
        await _repo.MigrateMedicalReportsAsync();

        // ── 1. UserType table ──────────────────────────────────────────────
        if (!await _sql.UserType.AnyAsync())
        {
            _sql.UserType.AddRange(
                new UserType { UserTypeName = "Admin" },
                new UserType { UserTypeName = "Blood Bank Staff" }
            );
            await _sql.SaveChangesAsync();
        }

        // Resolve UserType IDs by name (never hard-code identity values)
        var adminTypeId = await _sql.UserType
            .Where(t => t.UserTypeName == "Admin")
            .Select(t => t.UserTypeId)
            .FirstOrDefaultAsync();

        var staffTypeId = await _sql.UserType
            .Where(t => t.UserTypeName == "Blood Bank Staff")
            .Select(t => t.UserTypeId)
            .FirstOrDefaultAsync();

        // ── 2. Default admin ───────────────────────────────────────────────
        if (!await _sql.Users.AnyAsync(u => u.UserTypeId == adminTypeId))
        {
            await _repo.RegisterAsync(
                userTypeId:     adminTypeId,
                email:          "admin@redconnect.lk",
                password:       "Admin@123",
                name:           "System Admin",
                address:        "Colombo, Sri Lanka",
                nic:            "000000000V",
                donatedLng:     0, donatedLat:    0,
                availableLng:   0, availableLat:  0,
                locationSearch: "Colombo",
                phone:          "0000000000",
                gender:         GenderEnum.Male,
                bloodGroup:     ""
            );
        }

        // ── 3. Default Blood Bank Staff ────────────────────────────────────
        if (!await _sql.Users.AnyAsync(u => u.UserTypeId == staffTypeId))
        {
            await _repo.CreateOrUpdateBloodBankAsync(
                locationName: "RedConnect Central Blood Bank",
                address:      "Regent Street, Colombo 07, Sri Lanka",
                email:        "staff@redconnect.lk",
                password:     "Staff@123",
                userTypeId:   staffTypeId
            );
        }
    }
}
