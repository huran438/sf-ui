using System;

namespace SFramework.UI.Runtime
{
    public static partial class SFExtensions
    {
        public static bool HasParameters(this string[] parameters)
        {
            return parameters is { Length: > 0 };
        }

        public static bool HasParameter(this string[] parameters, string parameter)
        {
            return TryFindParameter(parameters, parameter, requireValue: false, out _);
        }

        public static bool TryGetParameterValue(this string[] parameters, string parameter, out string value)
        {
            return TryFindParameter(parameters, parameter, requireValue: true, out value);
        }

        private static bool TryFindParameter(string[] parameters, string key, bool requireValue, out string value)
        {
            value = null;
            if (!HasParameters(parameters) || string.IsNullOrEmpty(key)) return false;

            foreach (var entry in parameters)
            {
                if (entry == null) continue;
                var separator = entry.IndexOf('=');
                if (separator < 0 && requireValue) continue;
                var keyLength = separator < 0 ? entry.Length : separator;
                if (keyLength != key.Length || !entry.StartsWith(key, StringComparison.Ordinal)) continue;

                if (separator >= 0) value = entry.Substring(separator + 1);
                return true;
            }

            return false;
        }
    }
}
