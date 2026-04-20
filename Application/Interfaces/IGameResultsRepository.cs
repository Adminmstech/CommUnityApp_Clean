using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IGameResultsRepository
    {

        Task<(IEnumerable<dynamic> Data, int Total)> GetGamePlayMembers(int page, int size, string search);
    }
}
