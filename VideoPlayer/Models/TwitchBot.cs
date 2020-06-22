using System;
using System.Collections.Generic;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using System.Data;
using System.Reflection.Metadata;
using TwitchLib.Api.V5.Models.Ingests;
using System.ComponentModel.Design;
using VideoPlayer.Models;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils.Messaging;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System.Diagnostics;
using TwitchLib.PubSub.Interfaces;

namespace TestConsole
{
    public class TwitchBot
    {
        private const string KeyPhrase = "custom-reward-id=";
        private const string ValuePhraseExample = "8ea222f9-2469-4663-8f06-c8944e230ce4";
        private const string BotName = "mdalstrom";
        private const string Token = "rq3o94lis8puoa2vekg2vlzzks6f81";
        private const int IDLength = 11;

        private readonly TwitchClient _client;
        public string TargetChannel { get; set; }
        private List<string> _smorcsRequesters;

        public string OrderRewardID { get; set; }
        public string SkipRewardID { get; set; }

        private readonly List<string> _videoRequests;
        public IReadOnlyList<string> VideoRequests => _videoRequests;

        public event EventHandler<IMessage> Actioned;
        public TwitchBot(string targetChannel)
        {
            ConnectionCredentials credentials = new ConnectionCredentials(BotName, Token);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(1)
            };
            TargetChannel = targetChannel;
            _smorcsRequesters = new List<string>();
            _videoRequests = new List<string>();
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, targetChannel);
            _client.OnMessageReceived += Client_OnMessageReceived;
            _client.OnConnected += Client_OnConnected;
            _client.OnLog += Client_OnLog;
            _client.Connect();
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            if (e.Data.ToLower().Contains("custom-reward-id"))
            {
                HandleReward(e);
            }
        }
        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
        }
        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            HandleSmorcs(e);
        }

        #region Rewards
        private void HandleReward(OnLogArgs e)
        {
            if (e.Data == SkipRewardID)
            {
                RequestSkipVideo(SkipMessage.SkipSource.Award);
            }
            else if (e.Data == OrderRewardID)
            {
                string link;
                if (TryGetYoutubeURL(GetMessage(e.Data, TargetChannel), out link))
                {
                    AddVideo(link);
                }
                else
                {
                    _client.SendMessage(TargetChannel, $"Что-то не так с реквестом O_o");
                }
            }
            else
            {
                // Maybe any functionality on unexpected reward
            }
        }
        #endregion
        #region Skipping from chat
        private void HandleSmorcs(OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.ToLower() == "smorc")
            {
                if (!_smorcsRequesters.Contains(e.ChatMessage.DisplayName))
                {
                    AddVoice(e.ChatMessage);
                }
            }
            else if (e.ChatMessage.Message.ToLower() == "ezy")
            {
                ResetVoices(e.ChatMessage);
            }
        }
        private void AddVoice(ChatMessage msg)
        {
            _smorcsRequesters.Add(msg.DisplayName);
            Actioned?.Invoke(this, new VoteMessage
            {
                Username = msg.DisplayName,
                Reset = false,
                Count = _smorcsRequesters.Count
            });
        }
        private void ResetVoices(ChatMessage msg)
        {
            _smorcsRequesters.Clear();
            Actioned?.Invoke(this, new VoteMessage
            {
                Username = msg.DisplayName,
                Reset = true,
                Count = 0
            });
        }
        #endregion

        private void AddVideo(string url)
        {
            Actioned?.Invoke(this, new AddMessage
            { 
                Url = url
            });
        }
        private void RequestSkipVideo(SkipMessage.SkipSource source)
        {
            Actioned?.Invoke(this, new SkipMessage
            {
                Source = source,
                Url = _videoRequests[0]
            });
        }

        public static bool TryGetYoutubeURL(string source, out string result)
        {
            string id = "";
            var keyword = "";
            if (source.Contains("youtube.com"))
            {
                keyword = "v=";
                id = source.Substring(source.IndexOf(keyword) + keyword.Length, IDLength);
            }
            else if (source.Contains("youtu.be"))
            {
                keyword = "youtu.be/";
                id = source.Substring(source.IndexOf(keyword) + keyword.Length, IDLength);
            }
            else if (source.Length == IDLength)
            {
                id = source;
            }
            else
            {
                result = "Link is corrupted or just unidentified";
                return false;
            }
            result = $"https://youtube.com/embed/{id}";
            return true;
        }
        public static string GetRewardID(string source)
        {
            return source.Substring(source.IndexOf(KeyPhrase) + KeyPhrase.Length, ValuePhraseExample.Length);
        }
        public static string GetMessage(string source, string targetChannel)
        {
            string insertion = $"#{targetChannel} :";
            return source.Substring(source.IndexOf(insertion) + insertion.Length);
        }
    }

    [Serializable]
    public struct AddMessage : IMessage
    {
        [JsonProperty]
        public string Url { get; set; }
    }
    [Serializable]
    public struct SkipMessage : IMessage
    { 
        public enum SkipSource
        {
            Award,
            Orcs
        }
        [JsonProperty]
        public string Url { get; set; }
        [JsonProperty]
        public SkipSource Source { get; set; }
    }
    [Serializable]
    public struct VoteMessage : IMessage
    {
        [JsonProperty]
        public string Username { get; set; }
        [JsonProperty]
        public bool Reset { get; set; }
        [JsonProperty]
        public int Count { get; set; }
    }
    public interface IMessage
    {
        [JsonProperty]
        public string Type
        {
            get
            {
                string name = this.GetType().Name;
                return name.Substring(0, name.Length - "Message".Length);
            }
        }
    }
}