using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using OrangeBugReloaded.Core;
using OrangeBugReloaded.Core.ClientServer.Net;
using OrangeBugReloaded.Core.Entities;
using OrangeBugReloaded.Core.Tiles;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OrangeBugReloaded.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly Frame _frame;
        private string _port = "1234";
        private string _serverIp = "192.168.178.";

        public string Port
        {
            get { return _port; }
            set { Set(ref _port, value); }
        }

        public string ServerIp
        {
            get { return _serverIp; }
            set { Set(ref _serverIp, value); }
        }

        public string LocalIp { get; private set; }

        public MainViewModel()
        {
            if (IsInDesignMode)
                return;

            _frame = Window.Current.Content as Frame;
            DispatcherHelper.Initialize();
            LocalIp = GetLocalIp();
        }

        public void StartServer()
        {
            _frame.Navigate(typeof(GameServerPage), Port);
        }

        public void ConnectClient()
        {
            var serverInfo = new NetGameServerInfo(ServerIp, Port);
            _frame.Navigate(typeof(GameClientPage), serverInfo);
        }

        private string GetLocalIp()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();

            if (profile?.NetworkAdapter == null)
                return null;

            var hostNames = NetworkInformation.GetHostNames().Where(hn =>
                hn.IPInformation?.NetworkAdapter != null &&
                hn.IPInformation.NetworkAdapter.NetworkAdapterId == profile.NetworkAdapter.NetworkAdapterId);

            return hostNames.Any() ?
                string.Join("\r\n", hostNames.Select(hn => hn.CanonicalName)) :
                "(could not determine)";
        }

        #region Editor related
        private object _selectedTileTemplate;

        public object[] TileTemplates { get; } = new object[]
        {
            PathTile.Default,
            WallTile.Default,
            new ButtonTile(false, EntityFilterMode.Entities),
            new InkTile(InkColor.Red),
            new InkTile(InkColor.Green),
            new InkTile(InkColor.Blue),
            new PinTile(InkColor.Red),
            new PinTile(InkColor.Green),
            new PinTile(InkColor.Blue),
            BoxEntity.Default,
            new BalloonEntity(InkColor.Red),
            new BalloonEntity(InkColor.Green),
            new BalloonEntity(InkColor.Blue),
        };

        public object SelectedTileTemplate
        {
            get { return _selectedTileTemplate; }
            set { Set(ref _selectedTileTemplate, value); }
        }

        public async Task EditMapAsync(Vector2 canvasPosition)
        {
            /*var t = TransactionChainWithEditSupport.Create<TransactionWithEditSupport>(Map);
            var gamePosition = RendererClient.TransformCanvasPosition(canvasPosition);

            var meta = (SelectedTileTemplate is Tile) ?
                new TileMetadata((Tile)SelectedTileTemplate, 0) :
                new TileMetadata(Tile.Compose((await Map.GetMetadataAsync(gamePosition)).TileTemplate, (Entity)SelectedTileTemplate), 0);

            await t.SetMetadataAsync(gamePosition, meta);

            await t.CommitAsync(null);*/

            // Manual reset
            // TODO: Handle versioning correctly
            throw new NotImplementedException();
            //foreach (var kvp in t.CurrentTransaction.Changes)
            //{
            //    await Map.SetAsync(kvp.Key, kvp.Value.TileTemplate);
            //}
        }
        #endregion
    }
}
