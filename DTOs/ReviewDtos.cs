namespace QuanLyThucAnNhanh.DTOs
{
    public class ReviewDto
    {
        public long DanhGiaMonId { get; set; }
        public long NguoiDungId { get; set; }
        public int Diem { get; set; }
        public string? NhanXet { get; set; }
        public DateTime Ngay { get; set; }
    }

    public class CreateReviewDto
    {
        public long NguoiDungId { get; set; }
        public int Diem { get; set; }
        public string? NhanXet { get; set; }
    }

    public class PagedReviewsDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<ReviewDto> Items { get; set; } = new();
    }
}


