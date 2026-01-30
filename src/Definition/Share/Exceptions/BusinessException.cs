using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Share.Exceptions;

/// <summary>
/// throw new BusinessException when business error occurs
/// </summary>
/// <param name="LanguageKey">the key of language const</param>
/// <param name="statusCodes"></param>
/// <param name="arguments">optional arguments for parameterized messages</param>
[DebuggerNonUserCode]
public class BusinessException(
    string LanguageKey,
    int statusCodes = StatusCodes.Status500InternalServerError,
    params object[] arguments
) : Exception()
{
    public string LanguageKey { get; } = LanguageKey;
    public int StatusCodes { get; } = statusCodes;
    public object[] Arguments { get; } = arguments;
}
