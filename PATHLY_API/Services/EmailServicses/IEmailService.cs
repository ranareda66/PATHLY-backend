namespace PATHLY_API.Services.EmailServicses
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
