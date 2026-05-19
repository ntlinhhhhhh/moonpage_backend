using System;
using DiaryApp.Application.DTOs.Theme;
using DiaryApp.Domain.Entities;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Application.Interfaces.Services;

public interface IThemeService
{
    Task<IEnumerable<ThemeResponseDto>> GetAllActiveThemesAsync();
    Task<IEnumerable<ThemeResponseDto>> GetThemesByAuthorIdAsync(string authorId);
    Task<ThemeResponseDto?> GetThemeByIdAsync(string themeId);
    Task<IEnumerable<ThemeMoodResponseDto>> GetThemeMoodsAsync(string themeId);
    Task<ThemeMoodResponseDto?> GetMoodIconAsync(string themeId, BaseMood baseMoodId);
    Task CreateThemeAsync(CreateThemeRequestDto request);
    Task UpdateThemeAsync(string themeId, CreateThemeRequestDto request);
    Task DeleteThemeAsync(string themeId);

}
