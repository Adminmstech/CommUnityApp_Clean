using System.Data;
using CommUnityApp.ApplicationCore.Interfaces;
using CommUnityApp.ApplicationCore.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CommUnityApp.InfrastructureLayer.Repositories
{
    public class PushNotificationsDal : IPushNotificationsDal
    {
        private readonly IConfiguration _configuration;

        public PushNotificationsDal(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IReadOnlyList<PushNotificationRecipient>> GetRecipientsAsync(PushNotificationRecipientQuery query)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            var userIds = query.UserIds.Any()
                ? string.Join(",", query.UserIds)
                : null;

            var sql = @"
IF OBJECT_ID('dbo.SP_GetPushNotificationRecipients', 'P') IS NOT NULL
BEGIN
    EXEC dbo.SP_GetPushNotificationRecipients
        @Scope = @Scope,
        @UserIds = @UserIds,
        @CommunityId = @CommunityId,
        @EventId = @EventId,
        @BusinessId = @BusinessId;
END
ELSE
BEGIN
    IF @Scope = 'ExplicitUsers'
    BEGIN
        SELECT UserId, DeviceToken, 'ExplicitUsers' AS RecipientType
        FROM dbo.Users
        WHERE DeviceToken IS NOT NULL
          AND LTRIM(RTRIM(DeviceToken)) <> ''
          AND UserId IN (SELECT TRY_CONVERT(uniqueidentifier, value) FROM STRING_SPLIT(ISNULL(@UserIds, ''), ','));
    END
    ELSE IF @Scope = 'CommunityMembers'
    BEGIN
        SELECT DISTINCT UserId, DeviceToken, 'CommunityMembers' AS RecipientType
        FROM dbo.Users
        WHERE DeviceToken IS NOT NULL
          AND LTRIM(RTRIM(DeviceToken)) <> ''
          AND ISNULL(IsActive, 1) = 1
          AND EXISTS (
              SELECT 1
              FROM STRING_SPLIT(CONVERT(nvarchar(max), ISNULL(CommunityId, '')), ',') c
              WHERE TRY_CONVERT(int, LTRIM(RTRIM(c.value))) = @CommunityId
          );
    END
    ELSE IF @Scope = 'BusinessAdmin'
    BEGIN
        SELECT TOP (1) b.UserId, u.DeviceToken, 'BusinessAdmin' AS RecipientType
        FROM dbo.Business b
        INNER JOIN dbo.Users u ON u.UserId = b.UserId
        WHERE b.BusinessId = @BusinessId
          AND u.DeviceToken IS NOT NULL
          AND LTRIM(RTRIM(u.DeviceToken)) <> '';
    END
    ELSE IF @Scope = 'SuperAdmins'
    BEGIN
        SELECT UserId, DeviceToken, 'SuperAdmins' AS RecipientType
        FROM dbo.Users
        WHERE DeviceToken IS NOT NULL
          AND LTRIM(RTRIM(DeviceToken)) <> ''
          AND (Role = '1' OR Role LIKE '%,1,%' OR Role LIKE '1,%' OR Role LIKE '%,1');
    END
    ELSE IF @Scope = 'AllMembers'
    BEGIN
        SELECT UserId, DeviceToken, @Scope AS RecipientType
        FROM dbo.Users
        WHERE DeviceToken IS NOT NULL
          AND LTRIM(RTRIM(DeviceToken)) <> ''
          AND ISNULL(IsActive, 1) = 1;
    END
    ELSE
    BEGIN
        SELECT TOP (0) UserId, DeviceToken, @Scope AS RecipientType
        FROM dbo.Users;
    END
END";

            var recipients = await connection.QueryAsync<PushNotificationRecipient>(
                sql,
                new
                {
                    Scope = query.Scope.ToString(),
                    UserIds = userIds,
                    query.CommunityId,
                    query.EventId,
                    query.BusinessId
                });

            return recipients.ToList();
        }

        public async Task SaveDispatchLogAsync(PushNotificationTriggerRequest request, PushNotificationDispatchResult result)
        {
            try
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                await connection.ExecuteAsync(
                    @"IF OBJECT_ID('dbo.PushNotificationLogs', 'U') IS NOT NULL
                      BEGIN
                          INSERT INTO dbo.PushNotificationLogs
                              (TriggerName, Title, Body, RecipientCount, SuccessCount, FailureCount, CreatedDate)
                          VALUES
                              (@TriggerName, @Title, @Body, @RecipientCount, @SuccessCount, @FailureCount, GETDATE())
                      END",
                    new
                    {
                        TriggerName = request.Trigger.ToString(),
                        result.Title,
                        result.Body,
                        result.RecipientCount,
                        result.SuccessCount,
                        result.FailureCount
                    },
                    commandType: CommandType.Text);
            }
            catch
            {
                // Push delivery should not fail because an optional audit table is missing or unavailable.
            }
        }
    }
}
