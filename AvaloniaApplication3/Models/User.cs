using AvaloniaApplication3.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace AvaloniaApplication3.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(254)]
        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Member;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
