using Microsoft.Graphics.Canvas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using OB = OrangeBugReloaded.Core;

namespace OrangeBugReloaded.App.Presentation
{
    public class SpriteSheet
    {
        private readonly Dictionary<string, OB.Point> _names = new Dictionary<string, OB.Point>();
        private readonly CanvasBitmap _image;
        private readonly OB.Point _size;
        private readonly int _padding;
        private readonly OB.Point _spriteSize;

        public CanvasBitmap Image => _image;

        public SpriteSheet(CanvasBitmap image, IEnumerable<string> names, OB.Point size, int padding)
        {
            _image = image;
            _size = size;
            _padding = padding;

            _spriteSize = new OB.Point(
                ((int)image.SizeInPixels.Width - size.X * padding) / size.X,
                ((int)image.SizeInPixels.Height - size.Y * padding) / size.Y);

            var i = 0;

            foreach (var name in names)
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var x = i % size.X;
                    var y = i / size.X;
                    _names[name] = new OB.Point(x, y);
                }
                i++;
            }
        }

        /// <summary>
        /// Loads a spritesheet from an image file and
        /// a JSON description file.
        /// </summary>
        /// <param name="uri">
        /// Path to the spritesheet image file including
        /// file extension. The description file is determined
        /// by appending ".json" to this URI.
        /// </param>
        /// <returns>Sprite sheet</returns>
        public static async Task<SpriteSheet> LoadFromApplicationUriAsync(Uri uri, ICanvasResourceCreator resourceCreator)
        {
            var image = await CanvasBitmap.LoadAsync(resourceCreator, uri);

            var jsonUri = new Uri(uri.ToString() + ".json");
            var jsonFile = await StorageFile.GetFileFromApplicationUriAsync(jsonUri);
            var json = await FileIO.ReadTextAsync(jsonFile);

            var description = JsonConvert.DeserializeAnonymousType(json, new { Width = 0, Height = 0, Padding = 0, Names = new string[0] });
            return new SpriteSheet(image, description.Names, new OB.Point(description.Width, description.Height), description.Padding);
        }

        public void AddAlias(string alias, string mappedToName)
        {
            _names[alias] = _names[mappedToName];
        }

        public Rect this[string name]
        {
            get
            {
                OB.Point p;

                if (_names.TryGetValue(name, out p))
                    return this[p.X, p.Y];
                else
                    return this["NoSprite"];
            }
        }

        public Rect this[int x, int y]
        {
            get
            {
                return new Rect(
                    x * (_spriteSize.X + _padding) + _padding / 2,
                    y * (_spriteSize.Y + _padding) + _padding / 2,
                    _spriteSize.X,
                    _spriteSize.Y);
            }
        }
    }
}
