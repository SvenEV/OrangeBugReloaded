using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Storage;

namespace OrangeBugReloaded.App.Presentation
{
    public class AudioPlayer
    {
        private static AudioPlayer _instance;

        private Task _initialization;
        private Dictionary<Uri, AudioFileInputNode> _cachedSounds = new Dictionary<Uri, AudioFileInputNode>();
        private AudioGraph _graph;
        private AudioDeviceOutputNode _deviceOutput;

        public AudioPlayer()
        {
            _initialization = InitializeAsync();
        }

        public static async Task PlaySoundAsync(Uri uri)
        {
            if (_instance == null)
                _instance = new AudioPlayer();

            await _instance._initialization;
            await _instance.PlaySoundCoreAsync(uri);
        }

        private async Task PlaySoundCoreAsync(Uri uri)
        {
            AudioFileInputNode sound;

            if (!_cachedSounds.TryGetValue(uri, out sound))
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
                var result = await _graph.CreateFileInputNodeAsync(file);
                result.FileInputNode.Stop();
                result.FileInputNode.AddOutgoingConnection(_deviceOutput);
                sound = _cachedSounds[uri] = result.FileInputNode;
            }

            sound.Seek(TimeSpan.Zero);
            sound.Start();
        }

        private async Task InitializeAsync()
        {
            var settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.GameEffects);

            var graphCreationResult = await AudioGraph.CreateAsync(settings);
            _graph = graphCreationResult.Graph;

            var deviceOutputCreationResult = await _graph.CreateDeviceOutputNodeAsync();
            _deviceOutput = deviceOutputCreationResult.DeviceOutputNode;

            _graph.ResetAllNodes();
            _graph.Start();
        }
    }
}
