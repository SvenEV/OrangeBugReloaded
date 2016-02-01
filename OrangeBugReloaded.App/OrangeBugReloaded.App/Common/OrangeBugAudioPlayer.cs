using System;
using System.Collections.Generic;
using System.Linq;
using OrangeBugReloaded.Core;
using System.Reactive.Linq;
using OrangeBugReloaded.Core.Rendering;

namespace OrangeBugReloaded.App.Common
{
    public class OrangeBugAudioPlayer : IRendererPlugin
    {
        private Dictionary<string, Uri> _sounds = new Dictionary<string, Uri>();
        private IDisposable _eventSubscription;

        public void OnDraw(PluginDrawEventArgs e)
        {
        }

        public void Initialize(Map map)
        {
            _eventSubscription = map.Events.OfType<IAudioHint>().Subscribe(PlaySound);

            var soundUri = new Func<string, Uri>(s => new Uri($"ms-appx:///Assets/Sounds/{s}"));

            //_sounds["GateTile-Opened"] = soundUri("TODO");
            //_sounds["GateTile-Opened"] = soundUri("TODO");
            _sounds["InkTile-Consumed"] = soundUri("SplishSploshSplosh.mp3");
            _sounds["BalloonEntity-Popped"] = soundUri("Balloon Pop.mp3");
            _sounds["ButtonTile-Toggled-On"] = soundUri("Button Click.mp3");
            _sounds["ButtonTile-Toggled-Off"] = soundUri("Button Click Release.mp3");
            _sounds["TeleporterTile-Teleported"] = soundUri("teleport.mp3");
        }

        public void Dispose()
        {
            _eventSubscription.Dispose();
        }

        private async void PlaySound(IAudioHint audioHint)
        {
            Uri uri;

            if (_sounds.TryGetValue(audioHint.AudioKey, out uri))
            {
                await AudioPlayer.PlaySoundAsync(uri);
            }
        }
    }
}
