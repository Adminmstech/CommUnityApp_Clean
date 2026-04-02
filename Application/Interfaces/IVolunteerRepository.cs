using CommUnityApp.ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommUnityApp.ApplicationCore.Interfaces
{
    public interface IVolunteerRepository
    {

        Task<dynamic> VolunteerLogin(string email, string password);
        Task<List<VolunteerAssignedItemModel>> GetVolunteerAssignedRequests(Guid volunteerId);
        Task UpdateVolunteerRequestStatus(VolunteerStatusUpdateModel model);
    }
}
