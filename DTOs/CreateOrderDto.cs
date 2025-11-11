namespace QuanLyThucAnNhanh.DTOs
{
    public class CreateOrderItemDto
    {
        public long MonId { get; set; }
        public int SoLuong { get; set; }
    }

    public class CreateOrderDto
    {
        public long NguoiDungId { get; set; }
        public long ChiNhanhId { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}


