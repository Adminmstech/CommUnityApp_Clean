using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using CommUnityApp.Domain.Entities;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class ForgotPasswordRepository : IForgotPasswordRepository
    {
        private readonly IConfiguration _configuration;

        public ForgotPasswordRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<BaseResponseWithOtp> GenerateOtpAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<BaseResponseWithOtp>(
                "Generate_ForgotPassword_OTP",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new BaseResponseWithOtp
            {
                ResultId = 0,
                ResultMessage = "Failed to generate OTP"
            };
        }

        public async Task<BaseResponse> VerifyOtpAsync(string email, string otp)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentException("OTP cannot be null or empty", nameof(otp));

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email, DbType.String);
            parameters.Add("@OTP", otp, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>(
                "Verify_ForgotPassword_OTP",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new BaseResponse
            {
                ResultId = 0,
                ResultMessage = "OTP verification failed"
            };
        }

        public async Task<BaseResponse> ResetPasswordAsync(string email, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password cannot be null or empty", nameof(newPassword));

            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );

            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email, DbType.String);
            parameters.Add("@Password", newPassword, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>(
                "Reset_Password",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new BaseResponse
            {
                ResultId = 0,
                ResultMessage = "Password reset failed"
            };
        }
    }
}