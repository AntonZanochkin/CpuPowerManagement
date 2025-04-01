using Microsoft.Extensions.Logging;

namespace CpuPowerManagement.Services
{
  public class UserService : IUserService
  {
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
      _logger = logger;
    }

    public async Task SaveUserDataAsync()
    {
      _logger.LogInformation("Starting to save user data");
      await Task.Delay(1000);
      _logger.LogInformation("User data saved successfully");
    }
  }
}
