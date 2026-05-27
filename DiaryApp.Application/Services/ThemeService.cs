using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DiaryApp.Application.DTOs.Theme;
using DiaryApp.Domain.Entities;
using DiaryApp.Domain.Enums;
using DiaryApp.Application.Interfaces.Services;
using DiaryApp.Application.Interfaces;

namespace DiaryApp.Application.Interfaces.Services;

public class ThemeService(
    IThemeRepository themeRepository,
    IRedisCacheService cacheService,
    IGoogleStorageService googleStorageService,
    IMessageProducer messageProducer
    ) : IThemeService
{
    private readonly IThemeRepository _themeRepository = themeRepository;
    private readonly IRedisCacheService _cacheService = cacheService;
    private readonly IGoogleStorageService _googleStorageService = googleStorageService;
    private readonly IMessageProducer _messageProducer = messageProducer;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromDays(7);

    public async Task<IEnumerable<ThemeResponseDto>> GetAllActiveThemesAsync()
    {
        string cacheKey = "themes:all_active";

        var cachedThemes = await _cacheService.GetAsync<IEnumerable<ThemeResponseDto>>(cacheKey);
        if (cachedThemes != null) return cachedThemes;

        var themes = await _themeRepository.GetAllActiveThemesAsync();

        var dtos = themes.Select(theme => new ThemeResponseDto()
        {
            Id = theme.Id,
            Name = theme.Name,
            Price = theme.Price,
            ThumbnailUrl = _googleStorageService.GetImageUrl(theme.ThumbnailUrl ?? ""),
            BackgroundUrl = _googleStorageService.GetImageUrl(theme.BackgroundUrl ?? ""),
            BackgroundDarkColor = theme.BackgroundDarkColor,
            BackgroundLightColor = theme.BackgroundLightColor,
            PrimaryDarkColor = theme.PrimaryDarkColor,
            PrimaryLightColor = theme.PrimaryLightColor,
            AuthorId = theme.AuthorId,
            IsOfficial = theme.IsOfficial
        });

        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);

        return dtos;
    }

    public async Task<IEnumerable<ThemeResponseDto>> GetThemesByAuthorIdAsync(string authorId)
    {
        string cacheKey = $"themes:author:{authorId}";

        var cachedThemes = await _cacheService.GetAsync<IEnumerable<ThemeResponseDto>>(cacheKey);
        if (cachedThemes != null) return cachedThemes;

        var themes = await _themeRepository.GetThemesByAuthorIdAsync(authorId);

        var dtos = themes.Select(theme => new ThemeResponseDto()
        {
            Id = theme.Id,
            Name = theme.Name,
            Price = theme.Price,
            ThumbnailUrl = _googleStorageService.GetImageUrl(theme.ThumbnailUrl ?? ""),
            BackgroundUrl = _googleStorageService.GetImageUrl(theme.BackgroundUrl ?? ""),
            BackgroundDarkColor = theme.BackgroundDarkColor,
            BackgroundLightColor = theme.BackgroundLightColor,
            PrimaryDarkColor = theme.PrimaryDarkColor,
            PrimaryLightColor = theme.PrimaryLightColor,
            AuthorId = theme.AuthorId,
            IsOfficial = theme.IsOfficial
        });

        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);

        return dtos;
    }

    public async Task<ThemeResponseDto?> GetThemeByIdAsync(string themeId)
    {
        string cacheKey = $"theme:{themeId}";

        var cachedTheme = await _cacheService.GetAsync<ThemeResponseDto>(cacheKey);
        if (cachedTheme != null) return cachedTheme;

        var theme = await _themeRepository.GetByIdAsync(themeId);
        if (theme == null || !theme.IsActive)
        {
            return null;
        }
        var dto = new ThemeResponseDto
        {
            Id = themeId,
            Name = theme.Name,
            Price = theme.Price,
            ThumbnailUrl = _googleStorageService.GetImageUrl(theme.ThumbnailUrl ?? ""),
            BackgroundUrl = _googleStorageService.GetImageUrl(theme.BackgroundUrl ?? ""),
            BackgroundDarkColor = theme.BackgroundDarkColor,
            BackgroundLightColor = theme.BackgroundLightColor,
            PrimaryDarkColor = theme.PrimaryDarkColor,
            PrimaryLightColor = theme.PrimaryLightColor,
            AuthorId = theme.AuthorId,
            IsOfficial = theme.IsOfficial
        };

        await _cacheService.SetAsync(cacheKey, dto, _cacheTtl);
        return dto;
    }
    
    public async Task<ThemeMoodResponseDto?> GetMoodIconAsync(string themeId, BaseMood baseMoodId)
    {
        string cacheKey = $"theme_mood:{themeId}:{baseMoodId}";

        var cachedMood = await _cacheService.GetAsync<ThemeMoodResponseDto>(cacheKey);
        if (cachedMood != null) return cachedMood;

        var moodIcon = await _themeRepository.GetMoodIconAsync(themeId, baseMoodId);

        if (moodIcon == null) return null;

        var dto = new ThemeMoodResponseDto
        {
            BaseMoodId = baseMoodId.ToString(),
            IconColor = moodIcon.IconColor,
            CustomName = moodIcon.CustomName
        };

        await _cacheService.SetAsync(cacheKey, dto, _cacheTtl);
        return dto;
    }

    public async Task<IEnumerable<ThemeMoodResponseDto>> GetThemeMoodsAsync(string themeId)
    {
        string cacheKey = $"theme_moods_list:{themeId}";

        var cachedMoods = await _cacheService.GetAsync<IEnumerable<ThemeMoodResponseDto>>(cacheKey);
        if (cachedMoods != null) return cachedMoods;

        var theme = await _themeRepository.GetByIdAsync(themeId);
        
        if (theme == null || theme.Moods == null) 
            return Enumerable.Empty<ThemeMoodResponseDto>();

        var dtos = theme.Moods.Select(m => new ThemeMoodResponseDto
        {
            BaseMoodId = m.BaseMoodId.ToString(),
            IconColor = m.IconColor,
            CustomName = m.CustomName
        });

        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);
        return dtos;
    }

    public async Task CreateThemesListAsync(string authorId, List<CreateThemeRequestDto> requests)
    {
        foreach (var request in requests)
        {
            var existingTheme = await _themeRepository.GetByIdAsync(request.Id);
            if (existingTheme != null)
            {
                throw new InvalidOperationException($"A theme with the ID '{request.Id}' already exists.");
            }

            var newTheme = new Theme
            {
                Id = request.Id,
                Name = request.Name,
                Price = request.Price,
                ThumbnailUrl = request.ThumbnailUrl ?? "",
                BackgroundUrl = request.BackgroundUrl ?? "",
                BackgroundDarkColor = request.BackgroundDarkColor,
                BackgroundLightColor = request.BackgroundLightColor,
                PrimaryDarkColor = request.PrimaryDarkColor,
                PrimaryLightColor = request.PrimaryLightColor,
                AuthorId = authorId,
                IsOfficial = request.IsOfficial,
                IsActive = request.IsActive,
                Moods = request.Moods.Select(m => new ThemeMoodIcon
                {
                    BaseMoodId = m.BaseMoodId,
                    IconColor = m.IconColor,
                    CustomName = m.CustomName ?? ""
                }).ToList()
            };

            await _themeRepository.CreateThemeAsync(newTheme);
            await ClearThemeCachesAsync(request.Id, authorId);
        }
    }

    public async Task UploadThemeAsync(string authorId, UploadThemeRequestDto request)
    {
        var existingTheme = await _themeRepository.GetByIdAsync(request.Id);
        if (existingTheme != null)
        {
            throw new InvalidOperationException($"A theme with the ID '{request.Id}' already exists.");
        }

        var newTheme = new Theme
        {
            Id = request.Id,
            Name = request.Name,
            Price = request.Price,
            ThumbnailUrl = "pending",
            BackgroundUrl = "pending",
            BackgroundDarkColor = request.BackgroundDarkColor ?? "0xFFF4F6F1",
            BackgroundLightColor = request.BackgroundLightColor ?? "0xFF1C1C1C",
            PrimaryDarkColor = request.PrimaryDarkColor ?? "0xFFF4F6F1",
            PrimaryLightColor = request.PrimaryLightColor ?? "0xFF1C1C1C",
            AuthorId = authorId,
            IsOfficial = request.IsOfficial,
            IsActive = request.IsActive,
            Moods = DeserializeMoods(request.Moods)
        };

        await _themeRepository.CreateThemeAsync(newTheme);

        if (request.Thumbnail != null)
        {
            await SendImageToQueue(authorId, request.Id, request.Thumbnail, ImageUploadType.ThemeThumbnail);
        }

        if (request.Background != null)
        {
            await SendImageToQueue(authorId, request.Id, request.Background, ImageUploadType.ThemeBackground);
        }

        await ClearThemeCachesAsync(request.Id, authorId);
    }

    private async Task SendImageToQueue(string userId, string themeId, IFormFile file, ImageUploadType uploadType)
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), "moonpage_temp_images");
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);

        var fileName = $"{themeId}_{uploadType}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var tempFilePath = Path.Combine(tempFolder, fileName);

        using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var payload = new ImageUploadPayload
        {
            UserId = userId,
            EntityId = themeId,
            UploadType = uploadType,
            TempImagePath = tempFilePath
        };

        await _messageProducer.SendMessageAsync(payload, "image_upload_queue");
    }

    public async Task UpdateImageUrlAsync(string themeId, string imageUrl, bool isThumbnail)
    {
        var theme = await _themeRepository.GetByIdAsync(themeId);
        if (theme == null) return;

        if (isThumbnail) theme.ThumbnailUrl = imageUrl;
        else theme.BackgroundUrl = imageUrl;

        await _themeRepository.UpdateThemeAsync(theme);
        await ClearThemeCachesAsync(themeId, theme.AuthorId);
    }

    public async Task UpdateThemeAsync(string authorId, UploadThemeRequestDto request)
    {
        var theme = await _themeRepository.GetByIdAsync(request.Id);
        if (theme == null)
        {
            throw new KeyNotFoundException($"Theme with ID '{request.Id}' not found.");
        }

        theme.Name = request.Name;
        theme.Price = request.Price;
        if (!string.IsNullOrEmpty(request.BackgroundDarkColor)) theme.BackgroundDarkColor = request.BackgroundDarkColor;
        if (!string.IsNullOrEmpty(request.BackgroundLightColor)) theme.BackgroundLightColor = request.BackgroundLightColor;
        if (!string.IsNullOrEmpty(request.PrimaryDarkColor)) theme.PrimaryDarkColor = request.PrimaryDarkColor;
        if (!string.IsNullOrEmpty(request.PrimaryLightColor)) theme.PrimaryLightColor = request.PrimaryLightColor;
        theme.IsOfficial = request.IsOfficial;
        theme.IsActive = request.IsActive;

        if (request.Thumbnail != null)
        {
            await SendImageToQueue(authorId, request.Id, request.Thumbnail, ImageUploadType.ThemeThumbnail);
        }

        if (request.Background != null)
        {
            await SendImageToQueue(authorId, request.Id, request.Background, ImageUploadType.ThemeBackground);
        }

        if (!string.IsNullOrEmpty(request.Moods))
        {
            theme.Moods = DeserializeMoods(request.Moods);
        }

        await _themeRepository.UpdateThemeAsync(theme);
        await ClearThemeCachesAsync(request.Id, theme.AuthorId);
    }

    private List<ThemeMoodIcon> DeserializeMoods(string? moodsJson)
    {
        if (string.IsNullOrEmpty(moodsJson)) return new List<ThemeMoodIcon>();
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            
            var dtos = System.Text.Json.JsonSerializer.Deserialize<List<UploadThemeMoodDto>>(moodsJson, options);
            return dtos?.Select(m => new ThemeMoodIcon
            {
                BaseMoodId = m.BaseMoodId,
                IconColor = m.IconColor,
                CustomName = m.CustomName ?? ""
            }).ToList() ?? new List<ThemeMoodIcon>();
        }
        catch (Exception ex)
        {
            return new List<ThemeMoodIcon>();
        }
    }

    public async Task DeleteThemeAsync(string themeId)
    {
        var theme = await _themeRepository.GetByIdAsync(themeId);

        if (theme == null)
        {
            throw new KeyNotFoundException("The theme you are trying to delete doesn't exist.");
        }

        await _themeRepository.DeleteThemeAsync(themeId);
        await ClearThemeCachesAsync(themeId, theme.AuthorId);
    }

    private async Task ClearThemeCachesAsync(string themeId, string? authorId = null)
    {
        await _cacheService.RemoveAsync("themes:all_active");
        
        if (!string.IsNullOrEmpty(themeId))
        {
            if (string.IsNullOrEmpty(authorId))
            {
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme != null)
                {
                    authorId = theme.AuthorId;
                }
            }

            if (!string.IsNullOrEmpty(authorId))
            {
                await _cacheService.RemoveAsync($"themes:author:{authorId}");
            }

            await _cacheService.RemoveAsync($"theme:{themeId}");
            
            await _cacheService.RemoveAsync($"theme_moods_list:{themeId}");

            foreach (var mood in Enum.GetValues(typeof(BaseMood)))
            {
                await _cacheService.RemoveAsync($"theme_mood:{themeId}:{mood}");
            }
        }
    }
}