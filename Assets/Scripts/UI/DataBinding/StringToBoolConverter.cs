using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SvenVinkemeier.Unity.UI.DataBinding
{
    [AddComponentMenu("MVVM/Converters/String to Bool")]
    public class StringToBoolConverter : ValueConverter
    {

        public StringMatchMode MatchMode;
        public string MatchValue;

        public bool Invert;

        public override object Convert(object value)
        {
            var realResult = IsMatch(value);
            return realResult ? !Invert : Invert;
        }

        private bool IsMatch(object value)
        {
            switch (MatchMode)
            {
                case StringMatchMode.IsNullOrEmpty: return value == null || string.IsNullOrEmpty(value.ToString());
                case StringMatchMode.Equals: return value.ToString() == MatchValue;
                case StringMatchMode.StartsWith: return value.ToString().StartsWith(MatchValue);
                case StringMatchMode.EndsWith: return value.ToString().EndsWith(MatchValue);
                case StringMatchMode.Contains: return value.ToString().Contains(MatchValue);
                case StringMatchMode.MatchesRegex: return Regex.IsMatch(value.ToString(), MatchValue);
                default: throw new NotImplementedException();
            }
        }

        public override object ConvertBack(object value)
        {
            throw new NotImplementedException();
        }
    }

    public enum StringMatchMode
    {
        IsNullOrEmpty, Equals, StartsWith, EndsWith, Contains, MatchesRegex
    }
}