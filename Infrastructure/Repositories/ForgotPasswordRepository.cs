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

public class ForgotPasswordRepository : IForgotPasswordRepository
{
    private readonly IConfiguration _configuration;

    public ForgotPasswordRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    

    // ============================================
    // 1️⃣ Generate OTP
    // ============================================
    public async Task<BaseResponseWithOtp> GenerateOtpAsync(string email)
    {
        using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email, DbType.String);

        var result = await connection.QueryFirstOrDefaultAsync<BaseResponseWithOtp>(
            "Generate_ForgotPassword_OTP",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return result;
    }

    // ============================================
    // 2️⃣ Verify OTP
    // ============================================
    public async Task<BaseResponse> VerifyOtpAsync(string email, string otp)
    {
        using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email, DbType.String);
        parameters.Add("@OTP", otp, DbType.String);

        var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>(
            "Verify_ForgotPassword_OTP",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return result;
    }

    // ============================================
    // 3️⃣ Reset Password
    // ============================================
    public async Task<BaseResponse> ResetPasswordAsync(string email, string newPassword)
    {
        using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
        var parameters = new DynamicParameters();
        parameters.Add("@Email", email, DbType.String);
        parameters.Add("@Password", newPassword, DbType.String);

        var result = await connection.QueryFirstOrDefaultAsync<BaseResponse>(
            "Reset_Password",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return result;
    }

    public Task<ForgotPasswordRequest> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<ForgotPasswordRequest>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<int> AddAsync(ForgotPasswordRequest entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateAsync(ForgotPasswordRequest entity)
    {
        throw new NotImplementedException();
    }

    public Task<int> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}