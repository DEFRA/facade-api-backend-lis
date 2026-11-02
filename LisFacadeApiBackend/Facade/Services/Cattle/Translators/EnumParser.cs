using System.Reflection;
using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.Services.Cattle.Translators;

public static class EnumParser
{
    public static TEnum? ParseOrNull<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var candidate = value.Trim();

        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var jsonName = field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name;
            if (string.Equals(field.Name, candidate, StringComparison.OrdinalIgnoreCase)
                || string.Equals(jsonName, candidate, StringComparison.OrdinalIgnoreCase))
            {
                return (TEnum)field.GetValue(null)!;
            }
        }

        return null;
    }
}
