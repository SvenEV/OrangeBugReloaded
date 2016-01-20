using OrangeBugReloaded.Core.Designing;
using SvenVinkemeier.Unity.UI.DataBinding;
using System.Collections.Generic;
using System.Linq;

public class GameObjectDescriptionConverter : ValueConverter
{
    public override object Convert(object value)
    {
        var description = value as GameObjectDescription;
        var descriptionCollection = value as IEnumerable<GameObjectDescription>;

        if (description != null)
            return new CatalogItemViewModel(description);
        else if (descriptionCollection != null)
            return descriptionCollection.Select(o => new CatalogItemViewModel(o)).ToArray();
        else
            return null;
    }

    public override object ConvertBack(object value)
    {
        var vm = value as CatalogItemViewModel;
        return (vm == null) ? null : vm.GameObjectDescription;
    }
}