using DiaryApp.Domain.Entities;

namespace DiaryApp.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}