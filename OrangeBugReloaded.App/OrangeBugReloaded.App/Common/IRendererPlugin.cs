using Microsoft.Graphics.Canvas;
using OrangeBugReloaded.Core;
using System;
using System.Numerics;

namespace OrangeBugReloaded
{
    public interface IRendererPlugin : IDisposable
    {
        void Initialize(Map map);
        void OnDraw(CanvasDrawingSession g, Vector2 canvasSize);
        //Task OnCreateResourcesAsync(ICanvasResourceCreatorWithDpi resourceCreator);
    }
}
