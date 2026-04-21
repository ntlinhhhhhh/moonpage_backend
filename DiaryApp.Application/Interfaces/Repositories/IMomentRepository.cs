using DiaryApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiaryApp.Application.Interfaces;

public interface IMomentRepository
{
    // get moment_list of a user
    Task<Moment?> GetByIdAsync(string id);
    Task<IEnumerable<Moment>> GetMomentsByUserIdAsync(string userId);

    Task CreateAsync(Moment moment);
    
    Task UpdateAsync(Moment moment);

    Task DeleteAsync(string momentId);
    Task LinkMomentsToLogAsync(string userId, string dateStr, string logId);

    // update information of user
    Task SyncUserMediaInMomentsAsync(string userId, string newName, string newAvatarUrl);
}