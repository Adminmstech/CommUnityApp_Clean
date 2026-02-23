namespace CommUnityApp.ApplicationCore.Models
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }
    public class VerifyOtpRequest
    {
        public string Email { get; set; }
        public string OTP { get; set; }
    }
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
    public class BaseResponseWithOtp
    {
        public int ResultId { get; set; }
        public string ResultMessage { get; set; }
        public string OTP { get; set; }
    }
}
