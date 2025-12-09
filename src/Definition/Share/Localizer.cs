using Microsoft.Extensions.Localization;

namespace Share;

/// <summary>
/// 本地化资源
/// </summary>
public partial class Localizer(IStringLocalizer<Localizer> localizer)
{
    public string Get(string key, params object[] arguments)
    {
        try
        {
            return localizer[key, arguments];
        }
        catch (Exception)
        {
            return key;
        }

    }
}
