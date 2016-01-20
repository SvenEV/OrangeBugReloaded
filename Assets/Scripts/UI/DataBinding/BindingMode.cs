
namespace SvenVinkemeier.Unity.UI.DataBinding
{
    public enum SourceToTargetBindingMode
    {
        OneTime = 0,
        EventBased = 1,
        Periodically = 2,
        EveryFrame = 3
    }

    public enum TargetToSourceBindingMode
    {
        Disabled = 0,
        EventBased = 1
    }
}