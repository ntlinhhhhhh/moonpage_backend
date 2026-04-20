using DiaryApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiaryApp.Application.Interfaces;
public interface IActivityRepository
{
    Task CreateAsync(Activity activity);
    Task UpdateAsync(Activity activity);
    Task DeleteAsync(string activityId);

    Task<IEnumerable<Activity>> GetAllAsync();
    Task<IEnumerable<Activity>> GetByCategoryAsync(string category);
    Task<Activity?> GetByIdAsync(string id);
    public Task<bool> CheckAllActivitiesExistAsync(List<string> activityIds);

}