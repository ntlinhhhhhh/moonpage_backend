using System.IO;
using DiaryApp.Application.DTOs.Activity;
using DiaryApp.Application.Interfaces.Services;

namespace DiaryApp.Infrastructure.Data;

public class ActivitySeeder(
    ICloudinaryService cloudinaryService, 
    IActivityService activityService)
{
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
    private readonly IActivityService _activityService = activityService;

    // Lớp model phụ trợ để map dữ liệu dễ dàng hơn
    private class ActivitySeedModel
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }

    public async Task SeedActivitiesAsync()
    {
        // 1. Cấu hình danh sách các icon cần thêm từ ổ D của bạn
        // Lưu ý: Tôi đang giả định các file của bạn có đuôi là .png, 
        // nếu là .svg hay .jpg thì bạn nhớ sửa lại phần đuôi nhé!
        var activitiesToSeed = new List<ActivitySeedModel>
        {
            new() { Name = "Ăn sáng", Category = "Meal", FilePath = @"D:\icon\meal\breakfast.png" },
            new() { Name = "Ăn trưa", Category = "Meal", FilePath = @"D:\icon\meal\lunch.png" },
            new() { Name = "Ăn tối", Category = "Meal", FilePath = @"D:\icon\meal\dinner.png" },
            new() { Name = "Ăn đêm", Category = "Meal", FilePath = @"D:\icon\meal\night_snack.png" }
        };

        Console.WriteLine("🚀 Bắt đầu quá trình Seed dữ liệu Activity...");

        foreach (var item in activitiesToSeed)
        {
            if (!File.Exists(item.FilePath))
            {
                Console.WriteLine($"❌ Không tìm thấy file tại máy tính: {item.FilePath}");
                continue;
            }

            try
            {
                Console.WriteLine($"⏳ Đang upload {item.Name}...");

                // BƯỚC 1: Mở luồng đọc file và Upload lên Cloudinary
                using var stream = File.OpenRead(item.FilePath);
                
                string? cloudinaryUrl = await _cloudinaryService.UploadImageAsync(
                    stream, 
                    Path.GetFileName(item.FilePath), 
                    "activities"
                );

                if (string.IsNullOrEmpty(cloudinaryUrl))
                {
                    Console.WriteLine($"❌ Upload thất bại: {item.Name}");
                    continue;
                }

                // BƯỚC 2: Gọi ActivityService để lưu vào Database và tự động xóa Cache
                var requestDto = new ActivityRequestDto
                {
                    Name = item.Name,
                    Category = item.Category,
                    IconUrl = cloudinaryUrl
                };

                await _activityService.CreateActivityAsync(requestDto);

                Console.WriteLine($"✅ Thành công: {item.Name} -> {cloudinaryUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Lỗi hệ thống khi xử lý {item.Name}: {ex.Message}");
            }
        }

        Console.WriteLine("🏁 Hoàn thành việc đổ dữ liệu Activity mặc định!");
    }
}