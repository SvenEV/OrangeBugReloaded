using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace SvenVinkemeier.Unity.UI
{
    [AddComponentMenu("MVVM/Adapters/Dropdown Adapter")]
    public class DropdownAdapter : BindableMonoBehaviour
    {
        private Dropdown _dropdown;
        private bool _ignoreChange;
        private List<object> _items;

        public IEnumerable<object> Items
        {
            get { return _items; }
            set
            {
                _items = (value == null) ? null : value.ToList();

                if (_dropdown != null)
                {
                    _dropdown.options = (_items ?? Enumerable.Empty<object>())
                        .Select(item => new Dropdown.OptionData(item.ToString())).ToList();
                }
            }
        }

        public object SelectedItem
        {
            get { return (_items != null && _dropdown != null && _dropdown.value < _items.Count) ? _items[_dropdown.value] : null; }
            set
            {
                _ignoreChange = true;
                var index = (_items == null) ? -1 : _items.IndexOf(value);

                if (index != -1 && _dropdown != null)
                        _dropdown.value = index;

                _ignoreChange = false;
            }
        }

        private void Start()
        {
            _dropdown = GetComponent<Dropdown>();
            _dropdown.onValueChanged.AddListener(OnSelectedIndexChanged);
            _dropdown.options = (_items ?? Enumerable.Empty<object>()).Select(item => new Dropdown.OptionData(item.ToString())).ToList();
        }

        private void OnSelectedIndexChanged(int index)
        {
            if (!_ignoreChange)
                OnPropertyChanged("SelectedItem");
        }
    }
}