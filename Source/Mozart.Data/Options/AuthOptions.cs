using System.ComponentModel;
using System.Globalization;

namespace Mozart.Options;

[TypeConverter(typeof(AuthModeConverter))]
public enum AuthMode
{
    Default,
    Foreign
}

public class AuthOptions
{
    public const string Section = "Auth";

    public AuthMode Mode { get; init; } = AuthMode.Default;

    public int SessionExpiry { get; init; } = 5;

    public bool RevokeOnStartup { get; init; } = true;
}

public class AuthModeConverter() : EnumConverter(typeof(AuthMode))
{
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
        {
            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
            if (str.Equals("9you", comparison) || str.Equals("GAMANIA", comparison))
                return AuthMode.Foreign;
        }

        return base.ConvertFrom(context, culture, value);
    }
}
