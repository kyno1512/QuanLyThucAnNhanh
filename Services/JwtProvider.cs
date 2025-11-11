using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using QuanLyThucAnNhanh.Models;

namespace QuanLyThucAnNhanh.Services
{
    public class JwtProvider
    {
        private readonly IConfiguration _config;
        public JwtProvider(IConfiguration config) => _config = config;

        public string GenerateToken(NguoiDung user)
        {
            var claims = new[]
            {
                new Claim("UserId", user.NguoiDungId.ToString()),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim("VaiTro", user.VaiTro),
                new Claim("hoTen", user.HoTen ?? "") // <- thêm
            };

            var keyString = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new Exception("Jwt:Key not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
