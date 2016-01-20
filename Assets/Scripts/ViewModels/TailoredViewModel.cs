using OrangeBugReloaded.Core;
using SvenVinkemeier.Unity.UI;
using System;
using System.Collections;
using UnityEngine;

public abstract class TailoredViewModelBase : BindableMonoBehaviour
{
    private OrangeBugGameObjectViewModel _vm;
    private AudioSource _audio;

    protected AudioSource Audio
    {
        get
        {
            if (_audio == null)
                _audio = gameObject.AddComponent<AudioSource>();
            return _audio;
        }
    }

    protected string Sprite
    {
        get { return _vm.Sprite; }
        set { _vm.Sprite = value; }
    }

    public virtual void Initialize(OrangeBugGameObjectViewModel viewModel, OrangeBugGameObject obj)
    {
        _vm = viewModel;
    }

    protected void PlaySound(string name, bool delay = true)
    {
        if (delay)
            StartCoroutine(PlaySoundCore(SoundDictionary.Instance[name]));
        else
            Audio.PlayOneShot(SoundDictionary.Instance[name]);
    }

    private IEnumerator PlaySoundCore(AudioClip clip)
    {
        yield return new WaitForSeconds(.2f);
        Audio.PlayOneShot(clip);
    }
}

/// <summary>
/// A ViewModel that extends the functionality of a general ViewModel
/// like <see cref="LocationViewModel"/> or <see cref="EntityViewModel"/>
/// by being tailored to a specific <see cref="OrangeBugGameObject"/> type.
/// </summary>
/// <typeparam name="TViewModel">Type of the general ViewModel</typeparam>
/// <typeparam name="TObject">
/// Type the <see cref="TailoredViewModel{TViewModel, TObject}"/> is tailored to
/// </typeparam>
public class TailoredViewModel<TViewModel, TObject> : TailoredViewModelBase
    where TViewModel : OrangeBugGameObjectViewModel
    where TObject : OrangeBugGameObject
{
    protected TViewModel ViewModel { get; private set; }

    protected TObject Object { get; private set; }

    public override void Initialize(OrangeBugGameObjectViewModel viewModel, OrangeBugGameObject obj)
    {
        base.Initialize(viewModel, obj);

        if (!(viewModel is TViewModel))
            throw new ArgumentException(string.Format("The parameter has the wrong type (got '{0}', expected '{1}')", viewModel.GetType(), typeof(TViewModel), "viewModel"));

        if (!(obj is TObject))
            throw new ArgumentException(string.Format("The parameter has the wrong type (got '{0}', expected '{1}')", obj.GetType(), typeof(TObject), "obj"));

        ViewModel = (TViewModel)viewModel;
        Object = (TObject)obj;
        OnInitialize();
    }

    protected virtual void OnInitialize() { }
}