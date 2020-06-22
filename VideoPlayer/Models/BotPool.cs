using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestConsole;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace VideoPlayer.Models
{
    public class BotPool
    {
        private static BotPool _instance;
        public static BotPool Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BotPool();
                return _instance;
            }
        }
        private readonly List<TwitchBot> _bots;
        public IReadOnlyList<TwitchBot> Bots => _bots;
        public BotPool()
        {
            _bots = new List<TwitchBot>();
        }

        public event EventHandler<Package> Received;

        public int TryAdd(string channel)
        {
            if (_bots.Any(x => x.TargetChannel == channel))
            {
                return _bots.Count;
            }
            else
            {
                var bot = new TwitchBot(channel);
                bot.Actioned += (sender, message) =>
                {
                    Received?.Invoke(sender, new Package
                    {
                        User = channel,
                        Text = message
                    });
                };
                _bots.Add(bot);
                return _bots.Count;
            }
        }

        public TwitchBot GetBot(string channelName)
        {
            return _bots.Where(x => x.TargetChannel == channelName).FirstOrDefault();
        }
    }
    public struct Package
    {
        public string User { get; set; }
        public IMessage Text { get; set; }
        public string Json => JsonConvert.SerializeObject(this);
    }
    public struct NewBotMessage : IMessage
    {
        public string Username { get; set; }
    }
}
