using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using PlayDot.Utils.Vdf.Utilities;

namespace PlayDot.Utils.Vdf.Linq;

public static class Extensions
{
    public static T Value<T>(this IEnumerable<VToken> value)
    {
        return value.Value<VToken, T>();
    }

    public static T2 Value<T1, T2>(this IEnumerable<T1> value) where T1 : VToken
    {
        ValidationUtils.ArgumentNotNull(value, nameof(value));

        if (!(value is VToken token))
            throw new ArgumentException("Source value must be a JToken.");

        return token.Convert<VToken, T2>();
    }

    internal static T2 Convert<T1, T2>(this T1 token) where T1 : VToken
    {
        switch (token)
        {
            case null:
                return default;

            case T2
                // don't want to cast JValue to its interfaces, want to get the internal value
                when typeof(T2) != typeof(IComparable) && typeof(T2) != typeof(IFormattable):
                // HACK
                return (T2) (object) token;
        }

        if (token is not VValue value)
            throw new InvalidCastException($"Cannot cast {token.GetType()} to {typeof(T1)}.");

        if (value.Value is T2 u) return u;

        var targetType = typeof(T2);

        if (ReflectionUtils.IsNullableType(targetType))
        {
            if (value.Value == null)
                return default;

            targetType = Nullable.GetUnderlyingType(targetType);
        }

        if (TryConvertVdf(value.Value, out T2 resultObj)) return resultObj;

        Debug.Assert(targetType != null, nameof(targetType) + " != null");
        return (T2) System.Convert.ChangeType(value.Value, targetType, CultureInfo.InvariantCulture);
    }

    private static bool TryConvertVdf<T>(object value, out T result)
    {
        result = default;

        // It won't be null at this point, so just handle the nullable type.
        if ((typeof(T) == typeof(bool) || Nullable.GetUnderlyingType(typeof(T)) == typeof(bool)) &&
            value is string valueString)
            switch (valueString)
            {
                case "1":
                    result = (T) (object) true;
                    return true;

                case "0":
                    result = (T) (object) false;
                    return true;
            }

        return false;
    }
}