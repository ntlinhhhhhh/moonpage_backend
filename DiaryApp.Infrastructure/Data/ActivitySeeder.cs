// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using DiaryApp.Application.DTOs.Activity;
// using DiaryApp.Application.Interfaces.Services;

// namespace DiaryApp.Infrastructure.Data;

// public class ActivitySeeder(IActivityService activityService)
// {
//     private readonly IActivityService _activityService = activityService;

//     private class ActivitySeedModel
//     {
//         public string Name { get; set; } = string.Empty;
//         public string Category { get; set; } = string.Empty;
//         public string IconUrl { get; set; } = string.Empty;
//     }

//     public async Task SeedActivitiesAsync()
//     {
//         var activitiesToSeed = new List<ActivitySeedModel>
//         {
//             // Hobbies
//             new() { Name = "Exercise", Category = "Hobbies", IconUrl = "FitnessCenter" },
//             new() { Name = "TV & Content", Category = "Hobbies", IconUrl = "Tv" },
//             new() { Name = "Movie", Category = "Hobbies", IconUrl = "Movie" },
//             new() { Name = "Gaming", Category = "Hobbies", IconUrl = "SportsEsports" },
//             new() { Name = "Reading", Category = "Hobbies", IconUrl = "AutoStories" },
//             new() { Name = "Walk", Category = "Hobbies", IconUrl = "DirectionsWalk" },
//             new() { Name = "Music", Category = "Hobbies", IconUrl = "MusicNote" },
//             new() { Name = "Drawing", Category = "Hobbies", IconUrl = "Brush" },

//             // Emotions
//             new() { Name = "Excited", Category = "Emotions", IconUrl = "Celebration" },
//             new() { Name = "Relaxed", Category = "Emotions", IconUrl = "Spa" },
//             new() { Name = "Proud", Category = "Emotions", IconUrl = "EmojiEvents" },
//             new() { Name = "Hopeful", Category = "Emotions", IconUrl = "AutoAwesome" },
//             new() { Name = "Happy", Category = "Emotions", IconUrl = "SentimentVerySatisfied" },
//             new() { Name = "Enthusiastic", Category = "Emotions", IconUrl = "Whatshot" },
//             new() { Name = "Pit-a-pat", Category = "Emotions", IconUrl = "Favorite" },
//             new() { Name = "Refreshed", Category = "Emotions", IconUrl = "WaterDrop" },
//             new() { Name = "Calm", Category = "Emotions", IconUrl = "SelfImprovement" },
//             new() { Name = "Grateful", Category = "Emotions", IconUrl = "VolunteerActivism" },
//             new() { Name = "Depressed", Category = "Emotions", IconUrl = "SentimentVeryDissatisfied" },
//             new() { Name = "Lonely", Category = "Emotions", IconUrl = "PersonOutline" },
//             new() { Name = "Anxious", Category = "Emotions", IconUrl = "SentimentDissatisfied" },
//             new() { Name = "Sad", Category = "Emotions", IconUrl = "MoodBad" },
//             new() { Name = "Angry", Category = "Emotions", IconUrl = "PriorityHigh" },
//             new() { Name = "Pressured", Category = "Emotions", IconUrl = "Timer" },
//             new() { Name = "Annoyed", Category = "Emotions", IconUrl = "ErrorOutline" },
//             new() { Name = "Tired", Category = "Emotions", IconUrl = "Face" },
//             new() { Name = "Stressed", Category = "Emotions", IconUrl = "Psychology" },
//             new() { Name = "Bored", Category = "Emotions", IconUrl = "SentimentNeutral" },

//             // Meals
//             new() { Name = "Breakfast", Category = "Meals", IconUrl = "BreakfastDining" },
//             new() { Name = "Lunch", Category = "Meals", IconUrl = "LunchDining" },
//             new() { Name = "Dinner", Category = "Meals", IconUrl = "DinnerDining" },
//             new() { Name = "Night Snack", Category = "Meals", IconUrl = "Nightlight" },

//             // Self-Care
//             new() { Name = "Shower", Category = "Self-Care", IconUrl = "Shower" },
//             new() { Name = "Brush Teeth", Category = "Self-Care", IconUrl = "CleanHands" },
//             new() { Name = "Wash Face", Category = "Self-Care", IconUrl = "Face" },
//             new() { Name = "Drink Water", Category = "Self-Care", IconUrl = "LocalDrink" },

//             // Chores
//             new() { Name = "Cleaning", Category = "Chores", IconUrl = "CleaningServices" },
//             new() { Name = "Cooking", Category = "Chores", IconUrl = "Restaurant" },
//             new() { Name = "Laundry", Category = "Chores", IconUrl = "LocalLaundryService" },
//             new() { Name = "Dishes", Category = "Chores", IconUrl = "Kitchen" },

//             // Events
//             new() { Name = "Stay Home", Category = "Events", IconUrl = "Home" },
//             new() { Name = "School", Category = "Events", IconUrl = "School" },
//             new() { Name = "Restaurant", Category = "Events", IconUrl = "Restaurant" },
//             new() { Name = "Cafe", Category = "Events", IconUrl = "Coffee" },
//             new() { Name = "Shopping", Category = "Events", IconUrl = "ShoppingBag" },
//             new() { Name = "Travel", Category = "Events", IconUrl = "TravelExplore" },
//             new() { Name = "Party", Category = "Events", IconUrl = "Celebration" },
//             new() { Name = "Cinema", Category = "Events", IconUrl = "Theaters" },

//             // People
//             new() { Name = "Friends", Category = "People", IconUrl = "Group" },
//             new() { Name = "Family", Category = "People", IconUrl = "Groups" },
//             new() { Name = "Partner", Category = "People", IconUrl = "Favorite" },
//             new() { Name = "None", Category = "People", IconUrl = "PersonOff" },

//             // Beauty
//             new() { Name = "Hair", Category = "Beauty", IconUrl = "ContentCut" },
//             new() { Name = "Nails", Category = "Beauty", IconUrl = "Palette" },
//             new() { Name = "Skincare", Category = "Beauty", IconUrl = "Face" },
//             new() { Name = "Makeup", Category = "Beauty", IconUrl = "AutoFixHigh" },

//             // Weather
//             new() { Name = "Sunny", Category = "Weather", IconUrl = "WbSunny" },
//             new() { Name = "Cloudy", Category = "Weather", IconUrl = "Cloud" },
//             new() { Name = "Rainy", Category = "Weather", IconUrl = "Umbrella" },
//             new() { Name = "Snowy", Category = "Weather", IconUrl = "AcUnit" },
//             new() { Name = "Windy", Category = "Weather", IconUrl = "Air" },
//             new() { Name = "Stormy", Category = "Weather", IconUrl = "Thunderstorm" },
//             new() { Name = "Hot", Category = "Weather", IconUrl = "WbSunny" },
//             new() { Name = "Cold", Category = "Weather", IconUrl = "AcUnit" },

//             // Health
//             new() { Name = "Sick", Category = "Health", IconUrl = "Sick" },
//             new() { Name = "Hospital", Category = "Health", IconUrl = "LocalHospital" },
//             new() { Name = "Checkup", Category = "Health", IconUrl = "AssignmentTurnedIn" },
//             new() { Name = "Medicine", Category = "Health", IconUrl = "Medication" },

//             // Work
//             new() { Name = "Work", Category = "Work", IconUrl = "Work" },
//             new() { Name = "End on Time", Category = "Work", IconUrl = "AlarmOn" },
//             new() { Name = "Overtime", Category = "Work", IconUrl = "AccessTime" },
//             new() { Name = "Vacation", Category = "Work", IconUrl = "BeachAccess" },

//             // Other
//             new() { Name = "Snack", Category = "Other", IconUrl = "Cookie" },
//             new() { Name = "Coffee", Category = "Other", IconUrl = "Coffee" },
//             new() { Name = "Beverage", Category = "Other", IconUrl = "LocalDrink" },
//             new() { Name = "Tea", Category = "Other", IconUrl = "EmojiFoodBeverage" },
//             new() { Name = "Alcohol", Category = "Other", IconUrl = "LocalBar" },
//             new() { Name = "Smoking", Category = "Other", IconUrl = "SmokingRooms" },

//             // School
//             new() { Name = "Class", Category = "School", IconUrl = "CastForEducation" },
//             new() { Name = "Study", Category = "School", IconUrl = "AutoStories" },
//             new() { Name = "Homework", Category = "School", IconUrl = "EditNote" },
//             new() { Name = "Exam", Category = "School", IconUrl = "FactCheck" },

//             // Relationship
//             new() { Name = "Date", Category = "Relationship", IconUrl = "Favorite" },
//             new() { Name = "Anniversary", Category = "Relationship", IconUrl = "Cake" },
//             new() { Name = "Gift", Category = "Relationship", IconUrl = "CardGiftcard" },
//             new() { Name = "Conflict", Category = "Relationship", IconUrl = "Gavel" },
//             new() { Name = "Sex", Category = "Relationship", IconUrl = "BedroomParent" }
//         };

//         Console.WriteLine("🚀 Bắt đầu quá trình Seed dữ liệu Activity...");

//         foreach (var item in activitiesToSeed)
//         {
//             try
//             {
//                 Console.WriteLine($"⏳ Đang lưu {item.Name} vào database...");

//                 var requestDto = new ActivityRequestDto
//                 {
//                     Name = item.Name,
//                     Category = item.Category,
//                     IconUrl = item.IconUrl // Push thẳng ID dạng chuỗi này lên database
//                 };

//                 await _activityService.CreateActivityAsync(requestDto);

//                 Console.WriteLine($"✅ Thành công: {item.Name} -> {item.IconUrl}");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"⚠️ Lỗi hệ thống khi xử lý {item.Name}: {ex.Message}");
//             }
//         }

//         Console.WriteLine("🏁 Hoàn thành việc đổ dữ liệu Activity mặc định!");
//     }
// }