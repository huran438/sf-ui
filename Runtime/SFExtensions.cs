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
            if (!HasParameters(parameters)) return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].StartsWith(parameter)) ;
            }
            
            return false;
        }
        
        public static bool TryGetParameterValue(this string[] parameters, string parameter, out string value)
        {
            if (!HasParameters(parameters))
            {
                value = null;
                return false;
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].StartsWith(parameter))
                {
                    value = parameters[i].Split('=')[1];
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
