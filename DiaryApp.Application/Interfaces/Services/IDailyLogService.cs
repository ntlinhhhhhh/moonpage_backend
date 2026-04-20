using DiaryApp.Application.DTOs.DailyLog;

namespace DiaryApp.Application.Interfaces.Services;

public interface IDailyLogService
{
    Task UpsertLogAsync(string userId, DailyLogRequestDto request);
    Task<DailyLogResponseDto?> GetLogByDateAsync(string userId, string date);
    Task<IEnumerable<DailyLogResponseDto>> GetLogsByMonthAsync(string userId, string yearMonth);
    Task<IEnumerable<DailyLogResponseDto>> GetLogsByActivityAsync(string userId, string activityId, string yearMonth);
    Task<IEnumerable<DailyLogResponseDto>> GetLogsByMoodAsync(string userId, int moodId);
    Task<IEnumerable<DailyLogResponseDto>> GetLogsByMenstruationAsync(string userId, bool isMenstruation);
    Task<IEnumerable<DailyLogResponseDto>> SearchByNoteAsync(string userId, string keyword);
    Task DeleteLogAsync(string userId, string date);
}