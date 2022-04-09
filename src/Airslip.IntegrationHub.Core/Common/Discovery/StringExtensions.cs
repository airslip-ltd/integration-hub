using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Airslip.IntegrationHub.Core.Common.Discovery
{
    public static class StringExtensions
    {
        private static readonly Regex regEx = new(@"\{([\w'-]+)\}", RegexOptions.Compiled);
        public static string ApplyReplacements(this string format, Dictionary<string, string> replaceWith)
        {
            return regEx.Replace(format, delegate(Match match)
            {
                string key = match.Groups[1].Value;
                return replaceWith.ContainsKey(key) ? replaceWith[key] : string.Empty;
            });
        }
    }
}