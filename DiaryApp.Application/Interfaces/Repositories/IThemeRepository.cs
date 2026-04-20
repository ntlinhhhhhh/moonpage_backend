using DiaryApp.Domain.Entities;
using DiaryApp.Domain.Enums;

namespace DiaryApp.Application.Interfaces;

public interface IThemeRepository
{
    // get all theme of store 
    Task<IEnumerable<Theme>> GetAllActiveThemesAsync();

    // get detail a theme
    Task<Theme?> GetByIdAsync(string themeId);
    Task<ThemeMoodIcon?> GetMoodIconAsync(string themeId, BaseMood baseMoodId);
    
    // Admin/CMS 
    Task CreateThemeAsync(Theme theme);
    Task UpdateThemeAsync(Theme theme);
    Task DeleteThemeAsync(string themeId);
}