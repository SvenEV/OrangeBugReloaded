using UnityEngine;
using UnityEngine.UI;

namespace SvenVinkemeier.Unity.UI
{
    [AddComponentMenu("MVVM/Adapters/Toggle Adapter")]
    public class ToggleAdapter : BindableMonoBehaviour
    {
        private Toggle _toggle;
        private bool _isOn;
        private bool _ignoreChange;

        public bool IsOn
        {
            get { return _toggle.isOn; }
            set
            {
                _ignoreChange = true;
                _isOn = value;
                if (_toggle != null)
                    _toggle.isOn = value;
                _ignoreChange = false;
            }
        }

        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.isOn = _isOn;
            _toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            _toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnToggleValueChanged(bool _)
        {
            if (!_ignoreChange)
                OnPropertyChanged("IsOn");
        }
    }
}