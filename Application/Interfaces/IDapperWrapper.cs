using System.Data;
using System.Threading.Tasks;
using Dapper;
using System.Collections.Generic;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IDapperWrapper
    {
        Task<T> QueryFirstOrDefaultAsync<T>(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<IEnumerable<T>> QueryAsync<T>(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        Task<int> ExecuteAsync(IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
    }
}
