using OrangeBugReloaded.Core;
using System;

namespace OrangeBugReloaded.App.Common
{
    public interface IRendererPlugin : IDisposable
    {
        void Initialize(IGameplayMap map);
        void OnDraw(PluginDrawEventArgs e);
        //Task OnCreateResourcesAsync(ICanvasResourceCreatorWithDpi resourceCreator);
    }
}
