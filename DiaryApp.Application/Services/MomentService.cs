using DiaryApp.Application.DTOs.Moment;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Services;

public class MomentService(
    IMomentRepository momentRepository,
    IUserRepository userRepository,
    IRedisCacheService cacheService
    ) : IMomentService
{
    private readonly IMomentRepository _momentRepository = momentRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRedisCacheService _cacheService = cacheService;

    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(1);

    public async Task<MomentResponseDto> CreateMomentAsync(string userId, MomentRequestDto request, string imageUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("We couldn't find your account information.");

        var newMoment = new Moment
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            UserName = user.Name ?? "username",
            UserAvatarUrl = user.AvatarUrl ?? "",
            DailyLogId = request.DailyLogId,
            ImageUrl = imageUrl,
            Caption = request.Caption,
            IsPublic = request.IsPublic,
            CapturedAt = request.CapturedAt.ToUniversalTime()
        };

        await _momentRepository.CreateAsync(newMoment);

        await _cacheService.RemoveAsync($"moments_user:{userId}");

        return MapToResponseDto(newMoment);
    }

    public async Task<MomentResponseDto?> GetByIdAsync(string momentId)
    {
        string cacheKey = $"moment:{momentId}";
        var cachedMoment = await _cacheService.GetAsync<MomentResponseDto>(cacheKey);
        if (cachedMoment != null) return cachedMoment;

        var moment = await _momentRepository.GetByIdAsync(momentId);
        if (moment == null) return null;

        var dto = MapToResponseDto(moment);
        await _cacheService.SetAsync(cacheKey, dto, _cacheTtl);

        return dto;
    }

    public async Task<IEnumerable<MomentResponseDto>> GetMomentsByUserIdAsync(string userId)
    {
        string cacheKey = $"moments_user:{userId}";

        var cachedMoments = await _cacheService.GetAsync<IEnumerable<MomentResponseDto>>(cacheKey);
        if (cachedMoments != null) return cachedMoments;

        var moments = await _momentRepository.GetMomentsByUserIdAsync(userId);

        var dtos = moments.Select(MapToResponseDto).ToList();
        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);

        return dtos;
    }

    public async Task DeleteAsync(string userId, string momentId)
    {
        var moment = await _momentRepository.GetByIdAsync(momentId);
        if (moment == null) throw new KeyNotFoundException("We couldn't find this moment.");

        if (moment.UserId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete someone else's moment.");
        }

        await _momentRepository.DeleteAsync(momentId);
        await _cacheService.RemoveAsync($"moment:{momentId}");
        await _cacheService.RemoveAsync($"moments_user:{userId}");
    }

    private static MomentResponseDto MapToResponseDto(Moment moment)
    {
        return new MomentResponseDto
        {
            Id = moment.Id ?? string.Empty,
            UserId = moment.UserId,
            UserName = moment.UserName,
            UserAvatarUrl = moment.UserAvatarUrl,
            DailyLogId = moment.DailyLogId,
            ImageUrl = moment.ImageUrl,
            Caption = moment.Caption,
            IsPublic = moment.IsPublic,
            CapturedAt = moment.CapturedAt
        };
    }
}