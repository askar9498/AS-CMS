namespace AS_CMS.Domain.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
    Task SendEmailConfirmationAsync(string email, string confirmationToken);
    Task SendTwoFactorCodeAsync(string email, string code);
    Task SendAccountActivationEmailAsync(string email, string firstName);
    Task SendAccountDeactivationEmailAsync(string email, string firstName);
    Task SendRoleAssignmentEmailAsync(string email, string firstName, string roleName);
    Task SendProfileUpdateNotificationAsync(string email, string firstName);
} 