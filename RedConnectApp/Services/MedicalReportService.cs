using MongoDB.Driver;
using RedConnect.Interfaces;
using RedConnect.Models;

namespace RedConnectApp.Services
{
    public class MedicalReportService : IMedicalReportService
    {
        private readonly IMongoRepository _mongoRepo;
        private static readonly string[] _reportLabels =
       { "Blood Test Report", "Medical History", "Doctor's Certificate" };
        public MedicalReportService(IMongoRepository mongoRepo) 
        { 
            _mongoRepo = mongoRepo;
        }

        public async Task SaveMedicalReportsAsync(int userId, List<string> filePaths)
        {
            var reports = filePaths.Select((fp, i) => new MedicalReport
            {
                Index = i,
                Label = i < _reportLabels.Length ? _reportLabels[i] : $"Document {i + 1}",
                FilePath = fp,
                Status = "Pending"
            }).ToList();

            var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);
            var update = Builders<MongoUser>.Update
                .Set(x => x.MedicalReports, reports)
                .Set(x => x.DocumentsUploaded, true)
                .Set(x => x.LastUpdatedOn, DateTime.UtcNow);
            await _mongoRepo.UpdateAsync(update, filter);
        }

        public async Task ReuploadMedicalReportAsync(int userId, int docIndex, string filePath)
        {
            var user = await _mongoRepo.GetById(userId);
            if (user == null || docIndex < 0 || docIndex >= user.MedicalReports.Count) return;

            user.MedicalReports[docIndex].FilePath = filePath;
            user.MedicalReports[docIndex].Status = "Pending";
            user.MedicalReports[docIndex].RejectedReason = null;

            var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);
            var update = Builders<MongoUser>.Update
                .Set(x => x.MedicalReports, user.MedicalReports)
                .Set(x => x.LastUpdatedOn, DateTime.UtcNow);
            await _mongoRepo.UpdateAsync(update, filter);
        }
        public async Task UpdateMedicalReportStatusAsync(
        int userId, int docIndex, string status, string reason = null)
        {
            var user = await _mongoRepo.GetById(userId);
            if (user == null || docIndex < 0 || docIndex >= user.MedicalReports.Count) return;

            user.MedicalReports[docIndex].Status = status;
            user.MedicalReports[docIndex].RejectedReason = reason;

            var filter = Builders<MongoUser>.Filter.Eq(x => x.UserId, userId);
            var update = Builders<MongoUser>.Update
                .Set(x => x.MedicalReports, user.MedicalReports)
                .Set(x => x.LastUpdatedOn, DateTime.UtcNow);
            await _mongoRepo.UpdateAsync(update, filter);
        }
    }
}
