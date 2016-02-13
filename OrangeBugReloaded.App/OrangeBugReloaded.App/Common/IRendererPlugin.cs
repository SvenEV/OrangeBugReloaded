using OrangeBugReloaded.App.Common;
using OrangeBugReloaded.Core;
using System;

namespace OrangeBugReloaded
{
    public interface IRendererPlugin : IDisposable
    {
        void Initialize(IGameplayMap map);
        void OnDraw(PluginDrawEventArgs e);
        //Task OnCreateResourcesAsync(ICanvasResourceCreatorWithDpi resourceCreator);
    }
}
