using DiaryApp.Application.DTOs.Moment;

namespace DiaryApp.Application.Interfaces.Services;

public interface IMomentService
{
    Task<MomentResponseDto> CreateInitialMomentAsync(string userId, MomentRequestDto request);
    Task<MomentResponseDto> CreateMomentAsync(string userId, MomentRequestDto request, string imageUrl);
    Task UpdateImageUrlAsync(string momentId, string imageUrl);

    Task<MomentResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<MomentResponseDto>> GetMomentsByUserIdAsync(string userId);
    Task DeleteAsync(string userId, string momentId);
}