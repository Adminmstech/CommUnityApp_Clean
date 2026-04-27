using System.Data;
using System.Threading.Tasks;
using Dapper;
using System.Collections.Generic;
using CommUnityApp.ApplicationCore.Interfaces;

namespace CommUnityApp.InfrastructureLayer.Services
{
    public class DapperWrapper : IDapperWrapper
    {
        public async Task<T> QueryFirstOrDefaultAsync<T>(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await cnn.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await cnn.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public async Task<int> ExecuteAsync(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await cnn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }
    }
}