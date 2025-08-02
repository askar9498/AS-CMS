namespace AS_CMS.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    Task SendWelcomeEmailAsync(string to, string userName);
    Task SendPasswordResetEmailAsync(string to, string resetToken);
    Task SendEmailConfirmationAsync(string to, string confirmationToken);
    Task SendTwoFactorCodeAsync(string to, string code);
    Task SendAccountActivationEmailAsync(string to, string userName);
    Task SendAccountDeactivationEmailAsync(string to, string userName);
    Task SendRoleAssignmentEmailAsync(string to, string userName, string roleName);
    Task SendProfileUpdateNotificationAsync(string to, string userName);
} 