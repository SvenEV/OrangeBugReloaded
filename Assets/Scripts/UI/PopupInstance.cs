using SvenVinkemeier.Unity.UI;
using UnityEngine;

namespace SvenVinkemeier.Unity.UI
{
    public class PopupInstance : MonoBehaviour
    {
        public Popup Owner { get; set; }

        public void Hide() { Owner.Hide(); }
    }
}