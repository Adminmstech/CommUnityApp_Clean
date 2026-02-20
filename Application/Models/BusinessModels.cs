namespace CommUnityApp.ApplicationCore.Models
{
    public class BusinessLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class BusinessLoginResponse
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string Logo { get; set; }
        public string Email { get; set; }
        public string ResultMessage { get; set; }
    }

    public class AddUpdateBusinessRequest
    {
        public int BusinessId { get; set; }
        public int CategoryId { get; set; }
        public string BusinessName { get; set; }
        public string BusinessNumber { get; set; }
        public string OwnerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Suburb { get; set; }
        public string Info { get; set; }
        public string Logo { get; set; }
        public string Password { get; set; }
    }
}
