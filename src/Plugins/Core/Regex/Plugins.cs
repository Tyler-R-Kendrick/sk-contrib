using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SKHelpers.Plugins.Regex;

using Regex = System.Text.RegularExpressions.Regex;

public class CompiledRegexPlugin(Regex compiledRegex)
{
    [KernelFunction]
    [Description("Executes a regex pattern on the input string.")]
    [return: Description("The match result.")]
    public MatchCollection ExecuteRegexAsync(
        [Description("The input string.")]
        string input)
        => compiledRegex.Matches(input);
}

public class RegexPlugin
{
    [KernelFunction]
    [Description("Executes a regex pattern on the input string.")]
    [return: Description("The match result.")]
    public MatchCollection ExecuteRegexAsync(
        [Description("The compiled regex pattern.")]
        string regex,
        [Description("The input string.")]
        string input)
        => Regex.Matches(regex, input);
}
