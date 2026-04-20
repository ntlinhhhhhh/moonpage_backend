using System;
using DiaryApp.Application.DTOs.Theme;
using DiaryApp.Domain.Entities;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Application.Interfaces.Services;

public class ThemeService(
    IThemeRepository themeRepository,
    IRedisCacheService cacheService
    ) : IThemeService
{
    private readonly IThemeRepository _themeRepository = themeRepository;
    private readonly IRedisCacheService _cacheService = cacheService;
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
            ThumbnailUrl = theme.ThumbnailUrl,
            BackgroundUrl = theme.BackgroundUrl   
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
            ThumbnailUrl = theme.ThumbnailUrl,
            BackgroundUrl = theme.BackgroundUrl   
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
            IconUrl = moodIcon.IconUrl,
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
            IconUrl = m.IconUrl,
            CustomName = m.CustomName
        });

        await _cacheService.SetAsync(cacheKey, dtos, _cacheTtl);
        return dtos;
    }

    public async Task CreateThemeAsync(CreateThemeRequestDto request)
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
            IsActive = request.IsActive,
            Moods = request.Moods.Select(m => new ThemeMoodIcon
            {
                BaseMoodId = m.BaseMoodId,
                IconUrl = m.IconUrl,
                CustomName = m.CustomName ?? ""
            }).ToList()
        };

        await _themeRepository.CreateThemeAsync(newTheme);
        await ClearThemeCachesAsync(request.Id);
    }

    public async Task UpdateThemeAsync(string themeId, CreateThemeRequestDto request)
    {
        var existingTheme = await _themeRepository.GetByIdAsync(themeId);
        if (existingTheme == null)
        {
            throw new KeyNotFoundException($"We couldn't find a theme with the ID '{themeId}'.");
        }

        var updatedTheme = new Theme
        {
            Id = themeId,
            Name = request.Name,
            Price = request.Price,
            ThumbnailUrl = request.ThumbnailUrl ?? "",
            BackgroundUrl = request.BackgroundUrl ?? "",
            IsActive = request.IsActive,
            Moods = request.Moods.Select(m => new ThemeMoodIcon
            {
                BaseMoodId = m.BaseMoodId,
                IconUrl = m.IconUrl,
                CustomName = m.CustomName ?? ""
            }).ToList()
        };

        await _themeRepository.UpdateThemeAsync(updatedTheme);
        await ClearThemeCachesAsync(themeId);
    }

    public async Task DeleteThemeAsync(string themeId)
    {
        var theme = await _themeRepository.GetByIdAsync(themeId);

        if (theme == null)
        {
            throw new KeyNotFoundException("The theme you are trying to delete doesn't exist.");
        }

        await _themeRepository.DeleteThemeAsync(themeId);
        await ClearThemeCachesAsync(themeId);
    }

    private async Task ClearThemeCachesAsync(string themeId)
    {
        await _cacheService.RemoveAsync("themes:all_active");
        
        if (!string.IsNullOrEmpty(themeId))
        {
            await _cacheService.RemoveAsync($"theme:{themeId}");
            
            await _cacheService.RemoveAsync($"theme_moods_list:{themeId}");

            foreach (var mood in Enum.GetValues(typeof(BaseMood)))
            {
                await _cacheService.RemoveAsync($"theme_mood:{themeId}:{mood}");
            }
        }
    }
}