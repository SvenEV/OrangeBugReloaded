using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
using SvenVinkemeier.Unity.UI.DataBinding;

namespace SvenVinkemeier.Unity.UI
{
    [AddComponentMenu("MVVM/Adapters/Items Control")]
    public class ItemsControl : BindableMonoBehaviour
    {
        protected Dictionary<object, GameObject> _items = new Dictionary<object, GameObject>();

        public GameObject ItemTemplate;
        public DataTemplateSelector ItemTemplateSelector;

        public event Action<object, GameObject> ItemCreated;

        public IEnumerable<object> Items
        {
            get { return _items.Keys.ToArray(); }
            set
            {
                if (Equals(_items, value))
                    return;

                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }

                var dict = new Dictionary<object, GameObject>();

                if (value == null)
                    return;

                var i = 0;

                foreach (var item in value)
                {
                    var template = (ItemTemplateSelector != null) ? ItemTemplateSelector.SelectTemplate(item) : ItemTemplate;
                    if (template == null)
                        continue; // Items without template are not rendered

                    var newObject = Instantiate(template);
                    newObject.name = "#" + i++ + " (" + item.ToString() + ")";
                    newObject.transform.SetParent(transform, false);
                    newObject.AddComponent<DataContext>().DataContextObject = item;

                    if (ItemCreated != null)
                        ItemCreated(item, newObject);

                    dict[item] = newObject;
                }

                _items = dict;
            }
        }
    }
}