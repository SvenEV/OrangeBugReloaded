using OrangeBugReloaded.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrangeBugReloaded.App.Common
{
    public class PluginCollection
    {
        private OrangeBugRenderer _renderer;
        private List<Type> _pluginTypes = new List<Type>();
        private List<IRendererPlugin> _plugins = new List<IRendererPlugin>();

        public PluginCollection(OrangeBugRenderer renderer)
        {
            _renderer = renderer;
        }

        public bool Add<T>() where T : IRendererPlugin, new()
        {
            if (_pluginTypes.Contains(typeof(T)))
                return false;

            _pluginTypes.Add(typeof(T));

            if (_renderer.Map != null)
            {
                var plugin = new T();
                plugin.Initialize(_renderer.Map);
                _plugins.Add(plugin);
            }

            return true;
        }

        public bool Remove<T>() where T : IRendererPlugin, new()
        {
            if (!_pluginTypes.Contains(typeof(T)))
                return false;

            _pluginTypes.Remove(typeof(T));
            var plugin = _plugins.OfType<T>().First();
            plugin.Dispose();
            _plugins.Remove(plugin);
            return true;
        }

        internal void OnMapChanged(IGameplayMap newMap)
        {
            foreach (var plugin in _plugins)
                plugin.Dispose();

            _plugins.Clear();

            if (newMap != null)
            {
                _plugins.AddRange(_pluginTypes.Select(t => (IRendererPlugin)Activator.CreateInstance(t)));

                foreach (var plugin in _plugins)
                    plugin.Initialize(newMap);
            }
        }

        internal void RaiseOnDraw(PluginDrawEventArgs e)
        {
            foreach (var plugin in _plugins)
            {
                plugin.OnDraw(e);
            }
        }
    }
}
