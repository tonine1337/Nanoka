using System;
using System.ComponentModel.DataAnnotations;

namespace Nanoka.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UriAttribute : DataTypeAttribute
    {
        public UriAttribute() : base(DataType.Url) { }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            if (!(value is string str))
                return false;

            return Uri.IsWellFormedUriString(str, UriKind.Absolute);
        }
    }
}