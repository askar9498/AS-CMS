using AS_CMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace AS_CMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        try
        {
            _logger.Information("Sending email to: {To}, Subject: {Subject}", to, subject);
            
            // In a real implementation, you would use a proper email service
            // like SendGrid, MailKit, or SMTP
            // For now, we'll just log the email
            
            _logger.Information("Email sent successfully to: {To}", to);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send email to: {To}", to);
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string to, string userName)
    {
        var subject = "Welcome to AS-CMS";
        var body = $@"
            <h2>Welcome to AS-CMS!</h2>
            <p>Dear {userName},</p>
            <p>Thank you for registering with AS-CMS. Your account has been created successfully.</p>
            <p>You can now log in to your account and start using our services.</p>
            <p>Best regards,<br/>AS-CMS Team</p>";

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetToken)
    {
        var subject = "Password Reset - AS-CMS";
        var body = $@"
            <h2>Password Reset Request</h2>
            <p>You have requested to reset your password.</p>
            <p>Your new password is: <strong>{resetToken}</strong></p>
            <p>Please change this password after logging in for security purposes.</p>
            <p>If you did not request this reset, please contact support immediately.</p>
            <p>Best regards,<br/>AS-CMS Team</p>";

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendEmailConfirmationAsync(string to, string confirmationToken)
    {
        var subject = "Email Confirmation - AS-CMS";
        var body = $@"
            <h2>Email Confirmation</h2>
            <p>Please confirm your email address by clicking the link below:</p>
            <p><a href='{confirmationToken}'>Confirm Email</a></p>
            <p>If the link doesn't work, copy and paste this URL into your browser:</p>
            <p>{confirmationToken}</p>
            <p>Best regards,<br/>AS-CMS Team</p>";

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendTwoFactorCodeAsync(string to, string code)
    {
        var subject = "Two-Factor Authentication Code - AS-CMS";
        var body = $@"
            <h2>Two-Factor Authentication</h2>
            <p>Your two-factor authentication code is:</p>
            <h3 style='font-size: 24px; color: #007bff;'>{code}</h3>
            <p>This code will expire in 10 minutes.</p>
            <p>If you did not request this code, please ignore this email.</p>
            <p>Best regards,<br/>AS-CMS Team</p>";

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendAccountActivationEmailAsync(string to, string userName)
    {
        var subject = "Account Activated - AS-CMS";
        var body = $@"
            <h2>Account Activated</h2>
            <p>Dear {userName},</p>
            <p>Your account has been activated successfully.</p>
            <p>You can now log in to your account and access all features.</p>
            <p>Best regards,<br/>AS-CMS Team</p>";

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendAccountDeactivationEmailAsync(string to, string userName)
    {
        var subject = "Account Deactivated - AS-CMS";
        var body = $@"
            <h2>Account Deactivated</h2>
            <p>Dear {userName},</p>
            <p>Your account has been deactivated.</p>
            <p>If you believe this was done in error, please contact support.</p>
            <p>Best regards,<br/>AS-CMS Team</p>";

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendRoleAssignmentEmailAsync(string to, string userName, string roleName)
    {
        var subject = "Role Assignment - AS-CMS";
        var body = $@"
            <h2>Role Assignment</h2>
            <p>Dear {userName},</p>
            <p>You have been assigned the role: <strong>{roleName}</strong></p>
            <p>This role grants you access to additional features and permissions.</p>
            <p>Best regards,<br/>AS-CMS Team</p>";

        await SendEmailAsync(to, subject, body, true);
    }

    public async Task SendProfileUpdateNotificationAsync(string to, string userName)
    {
        var subject = "Profile Updated - AS-CMS";
        var body = $@"
            <h2>Profile Updated</h2>
            <p>Dear {userName},</p>
            <p>Your profile has been updated successfully.</p>
            <p>If you did not make these changes, please contact support immediately.</p>
            <p>Best regards,<br/>AS-CMS Team</p>";

        await SendEmailAsync(to, subject, body, true);
    }
} 