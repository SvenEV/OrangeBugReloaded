using UnityEngine;
using OrangeBugReloaded.Core;
using System.Collections;
using OrangeBugReloaded.Core.Foundation;
using System.Linq;
using SvenVinkemeier.Unity.UI;

public abstract class OrangeBugGameObjectViewModel : BindableMonoBehaviour
{
    // To be assigned by inspector
    public SpriteRenderer SpriteRenderer;

    private string _sprite;
    private TailoredViewModelBase _tailoredVM;

    public string Sprite
    {
        get { return _sprite; }
        set
        {
            _sprite = value;
            StartCoroutine(SetSprite(value));
        }
    }

    private IEnumerator SetSprite(string sprite)
    {
        if (SpriteRenderer.sprite != null && SpriteRenderer.sprite.name != "NoSprite")
            yield return new WaitForSeconds(.2f);

        SpriteRenderer.sprite = SpriteDictionary.Instance[sprite];
    }

    protected bool AddTailoredViewModel(OrangeBugGameObject modelObject)
    {
        // DEBUG
        if (modelObject is OrangeBugReloaded.Core.Tiles.Tile && Application.isEditor)
            gameObject.AddComponent<LockIndicator>();

        var viewModelType = Reflector.For<OrangeBugGameObjectViewModel>()
            .GetTypes<TailoredViewModelBase>()
            .FirstOrDefault(t =>
            {
                var attr = t.GetCustomAttribute<ViewModelAttribute>();
                return attr != null && attr.TargetType == modelObject.GetType();
            });

        if (viewModelType != null)
        {
            _tailoredVM = (TailoredViewModelBase)gameObject.AddComponent(viewModelType);
            _tailoredVM.Initialize(this, modelObject);
            return true;
        }

        return false;
    }

    protected void RemoveTailoredViewModel()
    {
        if (_tailoredVM != null)
            _tailoredVM.Dispose();
    }

    protected override void OnDispose()
    {
        RemoveTailoredViewModel();
        base.OnDispose();
    }
}
