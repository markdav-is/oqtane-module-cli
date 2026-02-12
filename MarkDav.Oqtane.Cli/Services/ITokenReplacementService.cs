namespace MarkDav.Oqtane.Cli.Services;

public interface ITokenReplacementService
{
    /// <summary>
    /// Generate a dictionary of token replacements based on module options.
    /// </summary>
    /// <param name="owner">Module owner/company name</param>
    /// <param name="moduleName">Module name</param>
    /// <param name="description">Module description</param>
    /// <param name="framework">Target framework (e.g., net10.0)</param>
    Dictionary<string, string> GenerateTokens(string owner, string moduleName, string description, string framework);

    /// <summary>
    /// Replace all tokens in the given content string.
    /// Note: [Guid] tokens generate a new GUID for each occurrence.
    /// </summary>
    string ReplaceTokens(string content, Dictionary<string, string> tokens);

    /// <summary>
    /// Replace tokens in a file or directory path.
    /// </summary>
    string ReplaceTokensInPath(string path, Dictionary<string, string> tokens);
}
