using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucAnNhanh.Models
{
    [Table("OTPEmails")]
    public class OTPEmail
    {
        [Column("OtpId")]   // map cột OtpId trong SQL
        public long OtpId { get; set; }

        public long NguoiDungId { get; set; }
        public string MucDich { get; set; } = string.Empty;
        public string MaOtp { get; set; } = string.Empty;
        public DateTime HetHanLuc { get; set; }
        public DateTime? XacMinhLuc { get; set; }

        public NguoiDung? NguoiDung { get; set; }
    }


}
