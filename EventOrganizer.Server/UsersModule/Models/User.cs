using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventOrganizer.Server.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        // Required fields
        public required string Name { get; set; }                     // Full name
        public required string Email { get; set; }                    // Login email
        public required string PasswordHash { get; set; }             // Hashed password
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional fields
        public bool IsVerified { get; set; } = false;        // Email verified
        public string Role { get; set; } = "User";           // "Admin", "User", etc.
        public string? PhoneNumber { get; set; }             // Optional phone
        public string? ProfilePictureUrl { get; set; }       // Link to avatar/profile image
        public DateTime? LastLogin { get; set; }             // Last login timestamp
        public Dictionary<string, string>? Preferences { get; set; }  // User settings/preferences


        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpires { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }
    }
}