using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using RabbitStreamMonitoring.Configuration;

namespace RabbitStreamMonitoring.Service
{
    public class MonitoringManager: IStartable, IDisposable
    {
        private readonly AppConfig _config;
        private List<ExchangeMonitoring> _monitorings = new List<ExchangeMonitoring>();
        private Timer _timer;

        public MonitoringManager(AppConfig config)
        {
            _config = config;
        }

        public void Start()
        {
            if (_config.MonitoringList == null)
                return;

            var bot = new Telegram.Bot.TelegramBotClient(_config.TelegramToken);
            var me = bot.GetMeAsync().Result;
            Console.WriteLine($"Telegram bot user name: {me.Username}");

            foreach (var item in _config.MonitoringList)
            {
                var monitoring = new ExchangeMonitoring(item, bot);
                _monitorings.Add(monitoring);
                monitoring.SayHello().Wait();
            }

            _timer = new Timer(DoTime, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        private void DoTime(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            foreach (var monitoring in _monitorings)
            {
                monitoring.CheckNotification().Wait();
            }

            _timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public void Dispose()
        {
            foreach (var monitoring in _monitorings)
            {
                monitoring.Dispose();
            }
        }
    }
}
