using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom_Texture_Importer.Utils
{
    public class RichPresenceClient
    {
        public static DiscordRpcClient Client { get; set; }

        private static RichPresence _currentPresence;

        private static readonly bool rpcIsEnabled = FortniteUtil.ConfigData.rpcIsEnabled;

        private static readonly Assets _assets = new Assets
        {
            LargeImageKey = "54129bd57b2f996b25c6759b9833f1e9",
            LargeImageText = "Custom Texture Importer (Made by Owen)"
        };

        private static readonly Timestamps _timestamps = new Timestamps
        {
            StartUnixMilliseconds = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds()
        };

        private static readonly Button[] _buttons =
        {
            new Button
            {
                Label = "Join our Discord",
                Url = "https://discord.gg/3hk59S4MQ3"
            },
            new Button
            {
                Label = "Visit our Github",
                Url = "https://github.com/owen-developer/Custom-Texture-Importer"
            }
        };

        public static void Start()
        {
            if (!rpcIsEnabled)
                return;

            Client = new("958805762455523358");

            _currentPresence = new RichPresence
            {
                Details = "Made by @owenonhxd",
                State = "Browsing for Texture...",
                Assets = _assets,
                Buttons = _buttons,
                Timestamps = _timestamps
            };

            Client.SetPresence(_currentPresence);
            Client.Initialize();
        }

        public static void UpdatePresence(string details, string State)
        {
            if (!rpcIsEnabled)
                return;

            if (!Client.IsInitialized)
                return;

            _currentPresence.Details = details;

            _currentPresence.State = State;

            Client.SetPresence(_currentPresence);
        }
    }
}
