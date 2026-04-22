using DiaryApp.Application.DTOs.Moment;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Services;

public class MomentService(
    IMomentRepository momentRepository,
    IUserRepository userRepository,
    IRedisCacheService cacheService,
    IMessageProducer messageProducer,
    IGoogleStorageService googleStorageService
    ) : IMomentService
{
    private readonly IMomentRepository _momentRepository = momentRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRedisCacheService _cacheService = cacheService;
    private readonly IMessageProducer _messageProducer = messageProducer;
    private readonly IGoogleStorageService _googleStorageService = googleStorageService;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromHours(1);

    public async Task<MomentResponseDto> CreateMomentAsync(string userId, MomentRequestDto request)
    {
        var initialMoment = await CreateInitialMomentAsync(userId, request);


        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "temp_images");
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.ImageFile.FileName)}";
            var tempFilePath = Path.Combine(tempFolder, fileName);

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await request.ImageFile.CopyToAsync(stream);
            }

            var payload = new ImageUploadPayload
            {
                UserId = userId,
                EntityId = initialMoment.Id,
                UploadType = ImageUploadType.Moment,
                TempImagePath = tempFilePath
            };

            await _messageProducer.SendMessageAsync(payload, "image_upload_queue");
        }
        return new MomentResponseDto
        {
            Id = initialMoment.Id,
            UserId = initialMoment.UserId,
            UserName = initialMoment.UserName, 
            UserAvatarUrl = initialMoment.UserAvatarUrl, 
            DailyLogId = initialMoment.DailyLogId,
            ImageUrl = string.Empty, 
            Caption = initialMoment.Caption,
            IsPublic = initialMoment.IsPublic,
            CapturedAt = initialMoment.CapturedAt
        };
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

    public async Task<MomentResponseDto> CreateInitialMomentAsync(string userId, MomentRequestDto request)
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
            DailyLogId = string.IsNullOrEmpty(request.DailyLogId) ? null : request.DailyLogId,
            ImageUrl = "pending",
            Caption = request.Caption,
            IsPublic = request.IsPublic,
            CapturedAt = request.CapturedAt.ToUniversalTime()
        };

        await _momentRepository.CreateAsync(newMoment);

        await _cacheService.RemoveAsync($"moments_user:{userId}");

        return MapToResponseDto(newMoment);
    }

    public async Task UpdateImageUrlAsync(string momentId, string imageUrl)
    {
        var moment = await _momentRepository.GetByIdAsync(momentId);
        if (moment == null)
            throw new KeyNotFoundException("Moment not found for update.");

        moment.ImageUrl = imageUrl;

        await _momentRepository.UpdateAsync(moment);

        await _cacheService.RemoveAsync($"moment:{momentId}");
        await _cacheService.RemoveAsync($"moments_user:{moment.UserId}");
    }

    private MomentResponseDto MapToResponseDto(Moment moment)
    {
        return new MomentResponseDto
        {
            Id = moment.Id ?? string.Empty,
            UserId = moment.UserId,
            UserName = moment.UserName,
            UserAvatarUrl = moment.UserAvatarUrl,
            DailyLogId = moment.DailyLogId,
            ImageUrl = _googleStorageService.GetImageUrl(moment.ImageUrl),
            Caption = moment.Caption,
            IsPublic = moment.IsPublic,
            CapturedAt = moment.CapturedAt
        };
    }
}