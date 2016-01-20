using UnityEngine;
using UnityEngine.UI;

namespace SvenVinkemeier.Unity.UI
{
    [AddComponentMenu("MVVM/Adapters/Input Field Adapter")]
    public class InputFieldAdapter : BindableMonoBehaviour
    {
        private InputField _input;
        private string _text;
        private bool _ignoreChange;

        public InputFieldUpdateMode UpdateMode = InputFieldUpdateMode.EndEdit;

        public string Text
        {
            get { return _input.text; }
            set
            {
                _ignoreChange = true;
                _text = value;
                if (_input != null)
                    _input.text = value ?? "";
                _ignoreChange = false;
            }
        }

        private void Start()
        {
            _input = GetComponent<InputField>();
            _input.text = _text ?? "";

            switch (UpdateMode)
            {
                case InputFieldUpdateMode.EndEdit:
                    _input.onEndEdit.RemoveListener(OnInputFieldTextChanged);
                    _input.onEndEdit.AddListener(OnInputFieldTextChanged);
                    break;

                case InputFieldUpdateMode.ValueChange:
                    _input.onValueChange.RemoveListener(OnInputFieldTextChanged);
                    _input.onValueChange.AddListener(OnInputFieldTextChanged);
                    break;
            }
        }

        private void OnInputFieldTextChanged(string _)
        {
            if (!_ignoreChange)
                OnPropertyChanged("Text");
        }

        public enum InputFieldUpdateMode
        {
            ValueChange,
            EndEdit
        }
    }
}