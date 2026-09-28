using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoreCRM.Infrastructure.Serialization
{
    public static class JsonValueConverter
    {
        public static object? ToClrValue(object? value)
        {
            if (value is not JsonElement element)
                return value;

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => null
            };
        }
    }
}
