using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using RabbitStreamMonitoring.Configuration;
using Swisschain.LykkeLog.Adapter;
using Telegram.Bot;

namespace RabbitStreamMonitoring.Service
{
    public class ExchangeMonitoring: IDisposable
    {
        private readonly RabbitMonitoring _config;
        private readonly TelegramBotClient _bot;
        private readonly RabbitMqSubscriber<byte[]> _connector;

        private DateTime _lastEvent = DateTime.MinValue;

        public ExchangeMonitoring(
            RabbitMonitoring config,
            TelegramBotClient bot)
        {
            _config = config;
            _bot = bot;


            var settings = new RabbitMqSubscriptionSettings()
            {
                ConnectionString = config.RabbitMqConnectionString,
                ExchangeName = config.RabbitMqExchange,
                QueueName = $"{config.RabbitMqExchange}.monitoring",
                IsDurable = false
            };
            _connector =
                new RabbitMqSubscriber<byte[]>(LegacyLykkeLogFactoryToConsole.Instance, settings, 
                        new DefaultErrorHandlingStrategy(LegacyLykkeLogFactoryToConsole.Instance, settings))
                    .SetMessageDeserializer(new MessageDeserializer())
                    .CreateDefaultBinding()
                    .Subscribe(HandleMessage)
                    .Start();
        }

        private Task HandleMessage(byte[] arg)
        {
            _lastEvent = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public async Task SayHello()
        {
            await _bot.SendTextMessageAsync(_config.ChatId, $"{_config.MonitoringName}: Start monitoring");
        }

        public async Task CheckNotification()
        {
            if (_config.IgnoreWeekend)
                if ((DateTime.UtcNow.DayOfWeek == DayOfWeek.Friday && DateTime.UtcNow.TimeOfDay.Hours >= 20) ||
                    (DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday) ||
                    (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday && DateTime.UtcNow.TimeOfDay.Hours <= 22))
                    return; 

            if ((DateTime.UtcNow - _lastEvent).TotalMinutes >= _config.WarningTimeoutInMinutes)
            {
                await _bot.SendTextMessageAsync(_config.ChatId, $"{_config.MonitoringName}: Do not receive events from RabbitMq last {DateTime.UtcNow - _lastEvent}");
            }
        }

        public class MessageDeserializer : IMessageDeserializer<byte[]>
        {
            public byte[] Deserialize(byte[] data)
            {
                return data.ToArray();
            }
        }

        public void Dispose()
        {
            _connector?.Stop();
            _connector?.Dispose();
        }
    }
}
