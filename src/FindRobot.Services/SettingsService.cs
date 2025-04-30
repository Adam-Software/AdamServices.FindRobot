using FindRobot.Interface;

namespace FindRobot.Services
{
    public class SettingsService : ISettingsService
    {
        public int Port { get; set; } = 11000;
    }
}
