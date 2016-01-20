using SvenVinkemeier.Unity.UI.DataBinding;
using UnityEngine;

namespace SvenVinkemeier.Unity.UI
{
    public class Popup : MonoBehaviour
    {
        private GameObject _popupInstance;
        
        public GameObject PopupTemplate;

        public void Toggle()
        {
            if (_popupInstance == null)
                Show();
            else
                Hide();
        }

        public void Show()
        {
            if (_popupInstance != null)
                return;

            _popupInstance = Instantiate(PopupTemplate);

            var dataContext = _popupInstance.GetComponent<DataContext>();
            if (dataContext == null) dataContext = _popupInstance.AddComponent<DataContext>();

            var companion = _popupInstance.GetComponent<PopupInstance>();
            if (companion == null) companion = _popupInstance.AddComponent<PopupInstance>();
            companion.Owner = this;

            dataContext.DataContextObject = BindingOperations.FindDataContext(transform);
            _popupInstance.transform.SetParent(GetComponentInParent<Canvas>().transform, false);
        }

        public void Hide()
        {
            if (_popupInstance != null)
                Destroy(_popupInstance);
        }

        private void OnDestroy()
        {
            Hide();
        }
    }
}