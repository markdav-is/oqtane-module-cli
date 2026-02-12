using System.Text.RegularExpressions;

namespace MarkDav.Oqtane.Cli.Services;

public class TokenReplacementService : ITokenReplacementService
{
    public Dictionary<string, string> GenerateTokens(string owner, string moduleName, string description, string framework)
    {
        return new Dictionary<string, string>
        {
            ["[Owner]"] = owner,
            ["[Module]"] = moduleName,
            ["[Description]"] = description,
            ["[Year]"] = DateTime.Now.Year.ToString(),
            ["[Date]"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["[Framework]"] = framework
        };
    }

    public string ReplaceTokens(string content, Dictionary<string, string> tokens)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        var result = content;

        // Replace all non-[Guid] tokens first
        foreach (var token in tokens)
        {
            result = result.Replace(token.Key, token.Value);
        }

        // Replace [Guid] tokens - each occurrence gets a unique GUID
        result = Regex.Replace(result, @"\[Guid\]", _ => Guid.NewGuid().ToString());

        return result;
    }

    public string ReplaceTokensInPath(string path, Dictionary<string, string> tokens)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        var result = path;

        // Only replace standard tokens in paths (not [Guid])
        foreach (var token in tokens)
        {
            result = result.Replace(token.Key, token.Value);
        }

        return result;
    }
}
