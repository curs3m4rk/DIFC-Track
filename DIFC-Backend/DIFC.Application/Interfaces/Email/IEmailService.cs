namespace DIFC.Application.Interfaces.Email
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink);
    }
}
