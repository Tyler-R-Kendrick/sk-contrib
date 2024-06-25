using System.ComponentModel;
using System.Globalization;
using Humanizer;
using Microsoft.SemanticKernel;

namespace SKHelpers.Plugins.Humanizer
{
    public class HumanizerPlugin
    {
        [KernelFunction]
        [Description("Humanizes a datetime to a readable string.")]
        [return: Description("The humanized datetime string.")]
        public string HumanizeDateTime(
            [Description("The datetime to humanize.")]
            DateTime dateTime,
            [Description("The culture for humanizing.")]
            string culture = "en-US") => dateTime.Humanize(culture: new CultureInfo(culture));

        [KernelFunction]
        [Description("Humanizes a timespan to a readable string.")]
        [return: Description("The humanized timespan string.")]
        public string HumanizeTimeSpan(
            [Description("The timespan to humanize.")]
            TimeSpan timeSpan,
            [Description("The culture for humanizing.")]
            string culture = "en-US") => timeSpan.Humanize(culture: new CultureInfo(culture));

        [KernelFunction]
        [Description("Humanizes a number to a readable string.")]
        [return: Description("The humanized number string.")]
        public string HumanizeNumber(
            [Description("The number to humanize.")]
            int number,
            [Description("The culture for humanizing.")]
            string culture = "en-US") => number.ToWords(new CultureInfo(culture));

        [KernelFunction]
        [Description("Dehumanizes a string to its original form.")]
        [return: Description("The dehumanized string.")]
        public string DehumanizeString(
            [Description("The string to dehumanize.")]
            string input) => input.Dehumanize();

        [KernelFunction]
        [Description("Pluralizes a word.")]
        [return: Description("The pluralized word.")]
        public string Pluralize(
            [Description("The word to pluralize.")]
            string word) => word.Pluralize();

        [KernelFunction]
        [Description("Singularizes a word.")]
        [return: Description("The singularized word.")]
        public string Singularize(
            [Description("The word to singularize.")]
            string word) => word.Singularize();

        [KernelFunction]
        [Description("Converts a number from a roman numeral.")]
        public int FromRoman(
            [Description("The roman numeral to convert.")]
            string roman) => roman.FromRoman();

        [KernelFunction]
        [Description("Changes word boundaries to dashes")]
        public string Kebaberize(
            [Description("The string to kebaberize.")]
            string input) => input.Kebaberize();

        [KernelFunction]
        [Description("Converts the word characters to the target case.")]
        public string ApplyCase(
            [Description("The string to apply case to.")]
            string input,
            [Description("The case to apply.")]
            LetterCasing casing) => input.ApplyCase(casing);

        [KernelFunction]
        [Description("Changes word boundaries to hyphens.")]
        public string Hyphenate(
            [Description("The string to hyphenate.")]
            string input) => input.Hyphenate();

        [KernelFunction]
        [Description("Changes word boundaries to underscores.")]
        public string UnderScore(
            [Description("The string to underscore.")]
            string input) => input.Underscore();

    }
}
