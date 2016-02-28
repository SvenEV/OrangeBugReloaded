using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using OrangeBugReloaded.App.Presentation;

namespace OrangeBugReloaded.App.Common
{
    public class PluginDrawEventArgs
    {
        public CanvasAnimatedDrawEventArgs Args { get; }

        public OrangeBugRenderer Renderer { get; }

        public PluginDrawEventArgs(CanvasAnimatedDrawEventArgs args, OrangeBugRenderer renderer)
        {
            Args = args;
            Renderer = renderer;
        }
    }
}
