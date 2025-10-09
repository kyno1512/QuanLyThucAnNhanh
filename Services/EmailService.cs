using System.Net;
using System.Net.Mail;

namespace QuanLyThucAnNhanh.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) => _config = config;

        public async Task SendOtpEmailAsync(string toEmail, string tenNguoiDung, string otp)
        {
            var host = _config["Smtp:Host"]!;
            var port = int.Parse(_config["Smtp:Port"]!);
            var user = _config["Smtp:User"]!;
            var pass = _config["Smtp:Pass"]!;
            var fromAddress = _config["Smtp:FromAddress"]!;
            var fromName = _config["Smtp:FromName"]!;

            using var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };

            string body = $"Xin chào {tenNguoiDung}, mã OTP của bạn là {otp}";

            var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = "Mã OTP xác nhận",
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            await smtp.SendMailAsync(message);
        }
    }
}
