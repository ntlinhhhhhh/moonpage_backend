using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiaryApp.Application.DTOs.Moment;
using DiaryApp.Application.Interfaces;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Domain.Entities;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Application.Services;

public class MomentService(
    IMomentRepository momentRepository,
    IUserRepository userRepository,
    IMessageProducer messageProducer,
    IGoogleStorageService googleStorageService,
    IRedisCacheService cacheService
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
        var responseDto = await CreateInitialMomentAsync(userId, request);

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), "moonpage_temp_images");
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
                EntityId = responseDto.Id,
                UploadType = ImageUploadType.Moment,
                TempImagePath = tempFilePath
            };

            await _messageProducer.SendMessageAsync(payload, "image_upload_queue");
            
            responseDto.ImageUrl = "processing";
        }

        return responseDto;
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
        await ClearMomentAndStatsCacheAsync(momentId, userId, moment.CapturedAt);
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
        await ClearMomentAndStatsCacheAsync(newMoment.Id, userId, newMoment.CapturedAt);

        return MapToResponseDto(newMoment);
    }

    public async Task UpdateImageUrlAsync(string momentId, string imageUrl)
    {
        var moment = await _momentRepository.GetByIdAsync(momentId);
        if (moment == null)
            throw new KeyNotFoundException("Moment not found for update.");

        moment.ImageUrl = imageUrl;
        await _momentRepository.UpdateAsync(moment);

        var keysToRemove = new List<string>
        {
            $"moment:{momentId}",
            $"moments_user:{moment.UserId}"
        };

        await Task.WhenAll(keysToRemove.Select(key => _cacheService.RemoveAsync(key)));
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

    private async Task ClearMomentAndStatsCacheAsync(string momentId, string userId, DateTime capturedDate)
    {
        int year = capturedDate.Year;
        int month = capturedDate.Month;

        var keysToRemove = new List<string>
        {
            $"moment:{momentId}",
            $"moments_user:{userId}",
            $"stats_summary:{userId}:{year}:{month}",
            $"stats_summary:{userId}:{year}:0"
        };

        await Task.WhenAll(keysToRemove.Select(key => _cacheService.RemoveAsync(key)));
    }
}