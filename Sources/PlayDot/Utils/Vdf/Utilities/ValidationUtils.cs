﻿using System;

namespace PlayDot.Utils.Vdf.Utilities;

internal static class ValidationUtils
{
    public static void ArgumentNotNull(object value, string parameterName)
    {
        if (value == null)
            throw new ArgumentNullException(parameterName);
    }
}