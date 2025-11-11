using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucAnNhanh.Models
{
    [Table("NhomMon")]
    public class NhomMon
    {
        [Key]
        public long NhomMonId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Ten { get; set; } = string.Empty;

        // Navigation property
        public List<Mon>? Mons { get; set; }
    }
}

