namespace Share.Services;

public interface ISystemConfigFacade
{
    Task<string?> GetValueAsync(string groupName, string key, CancellationToken cancellationToken = default);

    string RenderTemplate(string template, IReadOnlyDictionary<string, string> variables);
}
