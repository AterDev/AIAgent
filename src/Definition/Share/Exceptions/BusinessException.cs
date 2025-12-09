using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Share.Exceptions;

/// <summary>
/// throw new BusinessException when business error occurs
/// </summary>
/// <param name="LanguageKey">the key of language const</param>
/// <param name="statusCodes"></param>
[DebuggerNonUserCode]
public class BusinessException(
    string LanguageKey,
    int statusCodes = StatusCodes.Status500InternalServerError
) : Exception()
{
    public string LanguageKey { get; } = LanguageKey;
    public int StatusCodes { get; } = statusCodes;
}
