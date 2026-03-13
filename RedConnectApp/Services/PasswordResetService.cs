using RedConnectApp.DAL;
using RedConnectApp.Models;

namespace RedConnectApp.Services
{
    public class PasswordResetService
    {
        private readonly MSSQLDBContext _context;
        private readonly EmailService _emailService;

        public PasswordResetService(MSSQLDBContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public void SendResetLink(string email, string baseUrl)
        {
            var token = Guid.NewGuid().ToString();

            var resetToken = new PasswordResetToken
            {
                Email = email,
                Token = token,
                Expiry = DateTime.Now.AddMinutes(30)
            };

            _context.PasswordResetTokens.Add(resetToken);
            _context.SaveChanges();

            var link = $"{baseUrl}/Account/ResetPassword?token={token}";

            var body = $@"
                <h3>Password Reset</h3>
                <p>Click the link below to reset your password</p>
                <a href='{link}'>Reset Password</a>
            ";

            _emailService.SendEmail(email, "Password Reset", body);
        }

        public bool ResetPassword(string token, string newPassword)
        {
            var reset = _context.PasswordResetTokens
         .FirstOrDefault(x => x.Token == token && x.Expiry > DateTime.Now);

            if (reset == null)
                return false;

            var user = _context.Users.FirstOrDefault(x => x.Email == reset.Email);

            if (user == null)
                return false;

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _context.PasswordResetTokens.Remove(reset);

            _context.SaveChanges();

            return true;
        }
    }
}