using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SvenVinkemeier.Unity.UI.DataBinding;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace SvenVinkemeier.Unity.UI
{
    [AddComponentMenu("MVVM/Adapters/List View")]
    public class ListView : ItemsControl
    {
        private ToggleGroup _toggleGroup;
        private object _selectedItem;

        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (!Equals(value, _selectedItem))
                {
                    if (_toggleGroup != null)
                        _toggleGroup.SetAllTogglesOff();

                    // Select associated toggle
                    foreach (Transform toggle in transform)
                        if (Equals(toggle.GetComponent<DataContext>().DataContextObject, value))
                            toggle.GetComponent<Toggle>().isOn = true;

                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        public UnityEvent OnItemClick;

        private void Start()
        {
            _toggleGroup = GetComponent<ToggleGroup>() ?? gameObject.AddComponent<ToggleGroup>();

            ItemCreated += OnItemCreated;
            //var selectedItem = _toggleGroup.ActiveToggles().FirstOrDefault();
        }

        private void OnItemCreated(object dataItem, GameObject go)
        {
            var toggle = go.GetComponent<Toggle>();

            if (toggle == null)
                toggle = go.AddComponent<Toggle>();

            toggle.group = _toggleGroup;

            // Set up EventTrigger to trigger on item clicks
            var clickHandler = new EventTrigger.Entry();
            clickHandler.eventID = EventTriggerType.PointerClick;
            clickHandler.callback.AddListener(_ => OnItemClick.Invoke());
            go.AddComponent<EventTrigger>().triggers.Add(clickHandler);
        }

        private void Update()
        {
            var selectedToggle = _toggleGroup.ActiveToggles().FirstOrDefault();

            object item;

            if (selectedToggle == null)
            {
                item = null;
            }
            else
            {
                item = _items.First(o => Equals(o.Value, selectedToggle.gameObject)).Key;
            }

            if (item != _selectedItem)
            {
                _selectedItem = item;
                OnPropertyChanged("SelectedItem");
            }
        }
    }
}