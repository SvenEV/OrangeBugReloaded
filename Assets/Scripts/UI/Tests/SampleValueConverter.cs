using SvenVinkemeier.Unity.UI.DataBinding;
using System;

namespace SvenVinkemeier.Unity.UI.Tests
{
    public class SampleValueConverter : ValueConverter
    {
        public override object Convert(object value)
        {
            if (value == null)
                return value;

            var vm = (ColorItem)value;
            return vm.Color;
        }

        public override object ConvertBack(object value)
        {
            throw new NotImplementedException();
        }
    }
}