namespace RedConnect.Models
{
    public class MedicalReport
    {
        public int Index { get; set; }
        public string Label { get; set; }
        public string FilePath { get; set; }
        public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected
        public string RejectedReason { get; set; }
    }
}
