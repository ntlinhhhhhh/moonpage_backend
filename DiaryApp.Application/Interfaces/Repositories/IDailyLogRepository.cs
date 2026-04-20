using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Interfaces;

public interface IDailyLogRepository
{
    Task UpsertAsync(string userId, DailyLog log);
    Task<DailyLog?> GetByDateAsync(string userId, string date);
    Task<IEnumerable<DailyLog>> GetLogsByMonthAsync(string userId, string yearMonth);
    Task<IEnumerable<DailyLog>> GetLogsByMoodAsync(string userId, int moodId); 
    Task<IEnumerable<DailyLog>> GetLogsByActivityAsync(string userId, string activityId, string yearMonth);
    Task<IEnumerable<DailyLog>> GetLogsByMenstruationAsync(string userId, bool isMenstruation); // isMenstruation = true
    Task<IEnumerable<DailyLog>> SearchByNoteAsync(string userId, string keyword); // search by keyword of note
    Task DeleteAsync(string userId, string dateId);
}