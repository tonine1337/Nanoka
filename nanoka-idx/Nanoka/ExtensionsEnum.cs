using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Nanoka
{
    public class ExtensionsEnum<T>
        where T : struct
    {
        static Dictionary<string, T> _nameToValue;
        static Dictionary<T, string> _valueToName;

        static ExtensionsEnum()
        {
            _nameToValue = typeof(T).GetMembers()
                                    .Where(m => m.GetCustomAttribute<EnumMemberAttribute>() != null)
                                    .ToDictionary(m => m.GetCustomAttribute<EnumMemberAttribute>().Value,
                                                  m => Enum.Parse<T>(m.Name));

            _valueToName = _nameToValue.ToDictionary(x => x.Value, x => x.Key);
        }

        public static string GetName(T value) => _valueToName[value];
        public static T GetValue(string name) => _nameToValue[name];
    }
}