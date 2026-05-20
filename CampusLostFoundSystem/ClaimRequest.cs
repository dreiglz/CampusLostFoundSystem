namespace CampusLostFoundSystem
{
    public class ClaimRequest
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public string Category { get; set; } = "";
        public string ClaimerName { get; set; } = "";
        public string ProofDetails { get; set; } = "";
        public string RequestStatus { get; set; } = "Pending";
    }
}