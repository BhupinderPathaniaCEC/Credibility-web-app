using System;
using System.Text.RegularExpressions;

namespace CredibilityIndex.Application.Common;

public static class DomainUtility
{
    /// <summary>
    /// Normalizes a URL to a canonical domain (e.g., https://WWW.BBC.com/news -> bbc.com)
    /// </summary>
    public static string NormalizeDomain(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;

        // 1. Trim and Lowercase
        string domain = url.Trim().ToLowerInvariant();

        // 2. Extract Host if it's a full URL
        if (domain.Contains("://"))
        {
            try {
                domain = new Uri(domain).Host;
            } catch {
                // Fallback for malformed URLs: manually strip protocol
                domain = Regex.Replace(domain, @"^https?://", "");
            }
        }

        // 3. Remove trailing path/query if URI wasn't used
        int slashIndex = domain.IndexOf('/');
        if (slashIndex != -1) domain = domain.Substring(0, slashIndex);

        // 4. Strip leading 'www.' and trailing dots
        domain = domain.StartsWith("www.") ? domain.Substring(4) : domain;
        domain = domain.TrimEnd('.');

        return domain;
    }
}