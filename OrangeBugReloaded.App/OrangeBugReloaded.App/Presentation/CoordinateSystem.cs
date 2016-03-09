using OrangeBugReloaded.Core;
using System;
using System.Numerics;

namespace OrangeBugReloaded.App.Presentation
{
    class CoordinateSystem
    {
        /// <summary>
        /// Indicates how many device independent pixels
        /// make up a virtual game unit.
        /// </summary>
        public float DipsPerUnit { get; set; }

        /// <summary>
        /// Indicates how many virtual game units are
        /// covered by a device independent pixel.
        /// </summary>
        public float UnitsPerDip
        {
            get { return 1 / DipsPerUnit; }
            set { DipsPerUnit = 1 / DipsPerUnit; }
        }

        /// <summary>
        /// Zoom level.
        /// </summary>
        public float ZoomLevel { get; set; } = 1;

        /// <summary>
        /// Position of the camera in game coordinates.
        /// </summary>
        public Vector2 CameraPosition { get; set; } = Vector2.Zero;

        /// <summary>
        /// Canvas size in DIPs.
        /// </summary>
        public Vector2 CanvasSize { get; set; }

        /// <summary>
        /// Focus point of the camera between 0 and 1.
        /// </summary>
        public Vector2 Center { get; set; } = new Vector2(.5f, .5f);


        public Vector2 GameToCanvasPoint(Vector2 p)
        {
            return new Vector2(
                (p.X - CameraPosition.X - .5f) * ZoomLevel * DipsPerUnit + CanvasSize.X / 2,
                -(p.Y - CameraPosition.Y + .5f) * ZoomLevel * DipsPerUnit + CanvasSize.Y / 2);
        }

        public Vector2 CanvasToGamePoint(Vector2 p)
        {
            return new Vector2(
                (int)Math.Floor(((p.X - CanvasSize.X / 2) / ZoomLevel) + .5f + CameraPosition.X),
                (int)Math.Ceiling(((p.Y - CanvasSize.Y / 2) / -ZoomLevel) - .5f + CameraPosition.Y));
        }

        public Matrix3x2 GameToCanvasMatrix(Vector2 position, float rotation = 0)
        {
            return
                Matrix3x2.CreateScale(ZoomLevel) *
                Matrix3x2.CreateRotation(rotation, new Vector2(ZoomLevel * DipsPerUnit / 2)) *
                Matrix3x2.CreateTranslation(Center.X * CanvasSize.X, Center.Y * CanvasSize.Y) *
                Matrix3x2.CreateTranslation(-CameraPosition.X * ZoomLevel * DipsPerUnit, CameraPosition.Y * ZoomLevel * DipsPerUnit) *
                Matrix3x2.CreateTranslation(position.X * ZoomLevel * DipsPerUnit, -position.Y * ZoomLevel * DipsPerUnit) *
                Matrix3x2.Identity;
        }
    }
}
