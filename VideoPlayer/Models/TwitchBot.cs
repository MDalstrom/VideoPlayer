using System;
using System.Collections.Generic;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;

namespace TestConsole
{
    class TwitchBot
    {
        TwitchClient client;
        private List<string> _names;
        private int _orcs;
        public TwitchBot()
        {
            ConnectionCredentials credentials = new ConnectionCredentials("mdalstrom", "rq3o94lis8puoa2vekg2vlzzks6f81");
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, "untovvn");

            _names = new List<string>();

            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnConnected += Client_OnConnected;
            client.OnLog += Client_OnLog;
            client.Connect();
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            if (e.Data.ToLower().Contains("custom-reward-id"))
                Console.WriteLine($"{e.BotUsername} at {e.DateTime}: {e.Data}\n\n\n");
        }
        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            client.SendMessage(e.Channel, ":)");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.ToLower() == "smorc")
            {
                if (!_names.Contains(e.ChatMessage.DisplayName))
                {
                    _names.Add(e.ChatMessage.DisplayName);
                    _orcs++;
                }
                Console.WriteLine($"Voice for skip is submitted, current is {_orcs}");
                if (_orcs > 3)
                {
                    Console.WriteLine("Skipping");
                    client.SendMessage(e.ChatMessage.Channel, "СКИП ЗАСЧИТАН!");
                    _names.Clear();
                    _orcs = 0;
                }
            }
            else if (e.ChatMessage.Message.ToLower().Trim() == "ezy")
            {
                Console.WriteLine("Nulling the orcs");
                _names.Clear();
                _orcs = 0;
            }
            else if (e.ChatMessage.Message.ToLower().Trim() == "сбросить" && e.ChatMessage.DisplayName == "mdalstrom")
            {
                Console.WriteLine("Nulling the orcs by request");
                _names.Clear();
                _orcs = 0;
            }
            else if (e.ChatMessage.Message.Trim() == ":)")
            {
                Console.WriteLine(":) received");
            }
        }
    }
}