using System.Collections.Generic;

namespace RabbitStreamMonitoring.Configuration
{
    public class AppConfig
    {
        public List<RabbitMonitoring> MonitoringList { get; set; }

        public string TelegramToken { get; set; }
    }

    public class RabbitMonitoring
    {
        public string MonitoringName { get; set; }
        public string RabbitMqConnectionString { get; set; }
        public string RabbitMqExchange { get; set; }
        public bool IgnoreWeekend { get; set; }
        public int WarningTimeoutInMinutes { get; set; }
        public string ChatId { get; set; }
    }
}
