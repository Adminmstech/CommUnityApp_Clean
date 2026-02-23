using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IForgotPasswordRepository
    {
        Task<BaseResponseWithOtp> GenerateOtpAsync(string email);
        Task<BaseResponse> VerifyOtpAsync(string email, string otp);
        Task<BaseResponse> ResetPasswordAsync(string email, string newPassword);
    }
}
