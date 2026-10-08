namespace Perigon.AspNetCore.Options;

public class OAuthOption
{
    public const string ConfigPath = "Authentication:OAuth";
    public string Authority { get; set; } = string.Empty;
    public string[] Audiences { get; set; } = [];

    public bool RequireHttpsMetadata { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;

    public string? Sign { get; set; }
}
