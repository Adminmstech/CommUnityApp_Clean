namespace CommUnityApp.ApplicationCore.Models
{
    public class Users
    {
        public Guid? UserId { get; set; }         
        public int? CommunityId { get; set; }

        // Required Fields
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public string Mobile { get; set; }

        public string Password { get; set; }       
        public string RoleIds { get; set; }

        // 🔹 For receiving base64 image from frontend
        public string ProfileImageBase64 { get; set; }

        public string ProfileImagePath { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public string ZipCode { get; set; }
        public string City { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class RegisterRequest
    {
        public int CommunityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Mobile { get; set; }
        public string RoleIds { get; set; }
    }
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponse
    {
        public int ResultId { get; set; }

        public string ResultMessage { get; set; }
        public string Token { get; set; }
        public Guid UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Role { get; set; }   // Example: "2,3"

        public int IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class UserWallets
    {
        public int UserId { get; set; }
        public decimal Balance { get; set; }
        public int RewardCoins { get; set; }
    }

    public class WalletTransactions
    {
        public int WalletId { get; set; }
        public string TransactionType { get; set; }   // Credit / Debit
        public decimal Amount { get; set; }
        public int Coins { get; set; }
        public string ReferenceType { get; set; }     // Order / Booking / Recharge etc.
        public int ReferenceId { get; set; }
    }

    public class LoginUserResult
    {
        
        public int ResultId { get; set; }
        public string ResultMessage { get; set; } = string.Empty;

        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int IsActive { get; set; }
        public int BusinessId { get; set; }

    }

    public class  Roles
    {
        public int RoleId { get; set; }
        public string Role { get; set; }    

    }


    public class UserDropdownDto
    {
        public Guid UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Mobile { get; set; }

        public string Role { get; set; }
    }

}
