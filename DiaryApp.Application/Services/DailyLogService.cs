using DiaryApp.Application.DTOs.DailyLog;
using DiaryApp.Application.DTOs.Queue;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Services;

public class DailyLogService(
    IDailyLogRepository logRepository,
    IUserRepository userRepository,
    IActivityRepository activityRepository,
    IMomentRepository momentRepository,
    IRedisCacheService cacheService,
    IMessageProducer messageProducer
) : IDailyLogService
{
    private readonly IDailyLogRepository _logRepository = logRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IActivityRepository _activityRepository = activityRepository;
    private readonly IMomentRepository _momentRepository = momentRepository;
    private readonly IRedisCacheService _cacheService = cacheService;
    private readonly IMessageProducer _messageProducer = messageProducer;

    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(12);

    public async Task UpsertLogAsync(string userId, DailyLogRequestDto request)
    {
        await EnsureUserExistsAsync(userId);

        if (request.ActivityIds != null && request.ActivityIds.Any())
        {
            var exists = await _activityRepository.CheckAllActivitiesExistAsync(request.ActivityIds);
            if (!exists)
            {
                throw new KeyNotFoundException("One or more selected activities could not be found.");
            }
        }

        string extractedYearMonth = request.Date.Length >= 7 
            ? request.Date.Substring(0, 7) 
            : DateTime.UtcNow.ToString("yyyy-MM");

        var newLog = new DailyLog
            {
                Id = $"{userId}_{request.Date}",
                BaseMoodId = request.BaseMoodId,
                Date = request.Date,
                YearMonth = extractedYearMonth,
                Note = request.Note,
                SleepHours = request.SleepHours,
                IsMenstruation = request.IsMenstruation,
                MenstruationPhase = request.MenstruationPhase,
                DailyPhotos = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                ActivityIds = request.ActivityIds ?? new List<string>(),
            };

        await _logRepository.UpsertAsync(userId, newLog);
        await ClearLogCachesAsync(userId, request.Date, extractedYearMonth);

        if (request.DailyPhotos != null && request.DailyPhotos.Any())
        {
            var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "temp_images");
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

            foreach (var file in request.DailyPhotos)
            {
                if (file.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var tempFilePath = Path.Combine(tempFolder, fileName);

                    using (var stream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var payload = new ImageUploadPayload
                    {
                        UserId = userId,
                        EntityId = request.Date,
                        UploadType = ImageUploadType.DailyLog,
                        TempImagePath = tempFilePath
                    };

                    await _messageProducer.SendMessageAsync(payload, "image_upload_queue");
                }
            }
        }

        var dbPayload = new DatabaseTaskPayload
        {
            TaskType = DbTaskType.LinkMomentsToLog,
            UserId = userId,
            DateStr = request.Date,
            EntityId = newLog.Id
        };

        await _messageProducer.SendMessageAsync(dbPayload, "db_tasks_queue");
    }

    public async Task<DailyLogResponseDto?> GetLogByDateAsync(string userId, string date)
    {
        string cacheKey = $"log:{userId}:{date}";
        var cachedLog = await _cacheService.GetAsync<DailyLogResponseDto>(cacheKey);
        if (cachedLog != null) return cachedLog;

        await EnsureUserExistsAsync(userId);

        var log = await _logRepository.GetByDateAsync(userId, date);
        if (log == null) return null;

        var dto = MapToResponseDto(log);
        await _cacheService.SetAsync(cacheKey, dto, _cacheTtl);
        return dto;
    }

    public async Task<IEnumerable<DailyLogResponseDto>> GetLogsByMonthAsync(string userId, string yearMonth)
    {
        string cacheKey = $"logs_month:{userId}:{yearMonth}";
        var cachedLogs = await _cacheService.GetAsync<IEnumerable<DailyLogResponseDto>>(cacheKey);
        if (cachedLogs != null) return cachedLogs;

        await EnsureUserExistsAsync(userId);

        var logs = await _logRepository.GetLogsByMonthAsync(userId, yearMonth);

        var dtos = logs.Select(MapToResponseDto).ToList();
        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);

        return dtos;
    }

    public async Task<IEnumerable<DailyLogResponseDto>> GetLogsByActivityAsync(string userId, string activityId, string yearMonth)
    {
        await EnsureUserExistsAsync(userId);

        var logs = await _logRepository.GetLogsByActivityAsync(userId, activityId, yearMonth);
        return logs.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<DailyLogResponseDto>> GetLogsByMoodAsync(string userId, int moodId)
    {
        await EnsureUserExistsAsync(userId);
        var logs = await _logRepository.GetLogsByMoodAsync(userId, moodId);
        return logs.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<DailyLogResponseDto>> GetLogsByMenstruationAsync(string userId, bool isMenstruation)
    {
        await EnsureUserExistsAsync(userId);
        var logs = await _logRepository.GetLogsByMenstruationAsync(userId, isMenstruation);
        return logs.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<DailyLogResponseDto>> SearchByNoteAsync(string userId, string keyword)
    {
        await EnsureUserExistsAsync(userId);
        var logs = await _logRepository.SearchByNoteAsync(userId, keyword);
        return logs.Select(MapToResponseDto);
    }

    public async Task DeleteLogAsync(string userId, string date)
    {
        await EnsureUserExistsAsync(userId);

        var existingLog = await _logRepository.GetByDateAsync(userId, date);
        if (existingLog != null)
        {
            await _logRepository.DeleteAsync(userId, date);
            await ClearLogCachesAsync(userId, date, existingLog.YearMonth);
        }
    }

    public async Task AddPhotoToLogAsync(string userId, string date, string photoUrl)
    {
        await _logRepository.AddPhotoUrlAsync(userId, date, photoUrl);
        
        string extractedYearMonth = date.Length >= 7 ? date.Substring(0, 7) : DateTime.UtcNow.ToString("yyyy-MM");
        await ClearLogCachesAsync(userId, date, extractedYearMonth);
    }
    private static DailyLogResponseDto MapToResponseDto(DailyLog log)
    {
        return new DailyLogResponseDto
        {
            Id = log.Id,
            BaseMoodId = log.BaseMoodId,
            Date = log.Date,
            Note = log.Note,
            SleepHours = log.SleepHours,
            IsMenstruation = log.IsMenstruation,
            MenstruationPhase = log.MenstruationPhase,
            DailyPhotos = log.DailyPhotos ?? new List<string>(),
            ActivityIds = log.ActivityIds ?? new List<string>(),
            CreatedAt = log.CreatedAt
        };
    }

    private async Task EnsureUserExistsAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"We couldn't find a user with ID: {userId}"); 
        }
    }

    private async Task ClearLogCachesAsync(string userId, string date, string yearMonth)
    {
        await _cacheService.RemoveAsync($"log:{userId}:{date}");
        await _cacheService.RemoveAsync($"logs_month:{userId}:{yearMonth}");
    }
}