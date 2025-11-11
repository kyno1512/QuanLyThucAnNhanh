namespace QuanLyThucAnNhanh.DTOs
{
    public class RatingsSummaryDto
    {
        public decimal? AvgRating { get; set; }
        public int TotalReviews { get; set; }
        public List<StarCountDto> Distribution { get; set; } = new();
    }

    public class StarCountDto
    {
        public int Sao { get; set; }
        public int CountPerStar { get; set; }
    }
}


