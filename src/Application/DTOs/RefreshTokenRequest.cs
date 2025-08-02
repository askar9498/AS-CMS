using System.ComponentModel.DataAnnotations;

namespace AS_CMS.Application.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
} 