namespace RedConnect.Interfaces
{
    public interface IMedicalReportService
    {
        Task SaveMedicalReportsAsync(int userId, List<string> filePaths);
        Task ReuploadMedicalReportAsync(int userId, int docIndex, string filePath);

        Task UpdateMedicalReportStatusAsync(
        int userId, int docIndex, string status, string reason = null);
    }
}
