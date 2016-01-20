using System;
using System.Linq;
using UnityEngine;

namespace SvenVinkemeier.Unity.UI.DataBinding
{
    [AddComponentMenu("MVVM/Converters/Float to Color")]
    public class FloatToColorConverter : ValueConverter
    {
        [System.Serializable]
        public struct Key
        {
            public float KeyTime;
            public Color Color;
        }

        public Key[] Keys;

        public override object Convert(object value)
        {
            var v = (float)value;
            var before = Keys.Where(key => key.KeyTime <= v).OrderBy(key => v - key.KeyTime).ToArray();
            var after = Keys.Where(key => key.KeyTime >= v).OrderBy(key => key.KeyTime - v).ToArray();

            if (before.Length == 0)
            {
                if (after.Length == 0)
                    return default(Color);
                else
                    return after[0].Color;
            }
            else
            {
                if (after.Length == 0)
                    return before[0].Color;
                else
                    return Color.Lerp(before[0].Color, after[0].Color, Mathf.InverseLerp(before[0].KeyTime, after[0].KeyTime, v));
            }
        }

        public override object ConvertBack(object value)
        {
            throw new NotImplementedException();
        }
    }
}