public class CreateRatingResponse 
    {
        public string Message { get; set; } = string.Empty;
        public Guid WebsiteId { get; set; }
        public double AverageScore { get; set; }
        public DateTime ComputedAt { get; set; }
    }