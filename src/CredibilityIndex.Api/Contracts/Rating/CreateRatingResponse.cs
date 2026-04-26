public class CreateRatingResponse 
    {
        public string Message { get; set; } = string.Empty;
        public int WebsiteId { get; set; }
        public double AverageScore { get; set; }
        public DateTime ComputedAt { get; set; }
    }