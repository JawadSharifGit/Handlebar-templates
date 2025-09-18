using System.Globalization;
using System.Text.RegularExpressions;
using HandlebarsDotNet;
using HandlebarsEmailHelper.Interfaces;

namespace HandlebarsEmailHelper.Services;

// Interface moved to HandlebarsEmailHelper.Interfaces

public class TemplateService : ITemplateService
{
    private static readonly Regex PercentTokenRegex = new Regex("%(.*?)%", RegexOptions.Compiled);
    private readonly IHandlebars _handlebars;

    public TemplateService()
    {
        // HandlebarsDotNet already handles HTML escaping safely by default
        _handlebars = Handlebars.Create();
        RegisterHelpers();
    }

    public string TransformTokens(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return PercentTokenRegex.Replace(input, m =>
        {
            var token = m.Groups[1].Value;

            // If token has ":" then split into helper + argument
            if (token.Contains(":"))
            {
                var parts = token.Split(new[] { ':' }, 2);
                var helper = parts[0].Trim();
                var arg = parts[1].Trim();

                // Wrap argument in quotes to be safe for Handlebars
                return $"{{{{{helper} \"{arg}\"}}}}";
            }

            // Otherwise simple helper
            return $"{{{{{token}}}}}";
        });
    }

    public bool ContainsHandlebarsTokens(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        return input.Contains("{{");
    }

    public string CompileTemplate(string templateText, object model)
    {
        var transformed = TransformTokens(templateText);
        var compiled = _handlebars.Compile(transformed);
        return compiled(model);
    }

    public string CompileTemplate(string templateText, object model, IDictionary<string, string>? partials)
    {
        var transformed = TransformTokens(templateText);
        // Use a fresh instance so per-request partials don't leak globally
        var hb = Handlebars.Create();
        // Carry over helpers
        RegisterHelpersOn(hb);
        if (partials != null)
        {
            foreach (var kvp in partials)
            {
                var partialTransformed = TransformTokens(kvp.Value);
                hb.RegisterTemplate(kvp.Key, partialTransformed);
            }
        }
        var compiled = hb.Compile(transformed);
        return compiled(model);
    }

    public void RegisterHelpers()
    {
        RegisterHelpersOn(_handlebars);
    }

    private static void RegisterHelpersOn(IHandlebars hb)
    {
        // Register static helpers
        hb.RegisterHelper("year", (output, context, arguments) =>
        {
            var culture = arguments.Length > 0 ? arguments[0]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);
            output.Write(DateTime.Now.Year.ToString(cultureInfo));
        });

        hb.RegisterHelper("currentDate", (output, context, arguments) =>
        {
            var format = arguments.Length > 0 ? arguments[0]?.ToString() ?? "yyyy-MM-dd" : "yyyy-MM-dd";
            var culture = arguments.Length > 1 ? arguments[1]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);
            output.Write(DateTime.Now.ToString(format, cultureInfo));
        });

        // Enhanced date formatting with culture support
        hb.RegisterHelper("formatDate", (output, context, arguments) =>
        {
            if (arguments.Length == 0) return;
            
            var dateValue = arguments[0];
            var format = arguments.Length > 1 ? arguments[1]?.ToString() ?? "yyyy-MM-dd" : "yyyy-MM-dd";
            var culture = arguments.Length > 2 ? arguments[2]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);

            if (DateTime.TryParse(dateValue?.ToString(), out var date))
            {
                output.Write(date.ToString(format, cultureInfo));
            }
            else
            {
                output.Write(dateValue?.ToString() ?? string.Empty);
            }
        });

        // Enhanced number formatting with culture support
        hb.RegisterHelper("formatNumber", (output, context, arguments) =>
        {
            if (arguments.Length == 0) return;
            
            var numberValue = arguments[0];
            var format = arguments.Length > 1 ? arguments[1]?.ToString() ?? "N2" : "N2";
            var culture = arguments.Length > 2 ? arguments[2]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);

            if (decimal.TryParse(numberValue?.ToString(), out var number))
            {
                output.Write(number.ToString(format, cultureInfo));
            }
            else if (double.TryParse(numberValue?.ToString(), out var doubleNumber))
            {
                output.Write(doubleNumber.ToString(format, cultureInfo));
            }
            else
            {
                output.Write(numberValue?.ToString() ?? string.Empty);
            }
        });

        hb.RegisterHelper("appName", (output, context, arguments) =>
        {
            output.Write("HandleBars");
        });

        hb.RegisterHelper("Name", (output, context, arguments) =>
        {
            output.Write("HandleBar");
        });

        hb.RegisterHelper("Company", (output, context, arguments) =>
        {
            output.Write("HandleBars Company");
        });

        hb.RegisterHelper("Email", (output, context, arguments) =>
        {
            output.Write("handle@Bars.io");
        });

        // Conditional block helper
        hb.RegisterHelper("ifNotEmpty", (output, options, context, arguments) =>
        {
            if (arguments.Length > 0 && !string.IsNullOrEmpty(arguments[0]?.ToString()))
            {
                options.Template(output, context);
            }
            else
            {
                options.Inverse(output, context);
            }
        });

        // String manipulation helpers
        hb.RegisterHelper("uppercase", (writer, context, parameters) =>
        {
            if (parameters.Length == 0) return;
            var value = parameters[0]?.ToString() ?? string.Empty;
            var culture = parameters.Length > 1 ? parameters[1]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);
            writer.WriteSafeString(System.Net.WebUtility.HtmlEncode(value.ToUpper(cultureInfo)));
        });

        hb.RegisterHelper("lowercase", (writer, context, parameters) =>
        {
            var value = parameters.Length > 0 ? parameters[0]?.ToString() ?? string.Empty : string.Empty;
            var culture = parameters.Length > 1 ? parameters[1]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);
            writer.WriteSafeString(System.Net.WebUtility.HtmlEncode(value.ToLower(cultureInfo)));
        });

        hb.RegisterHelper("titleCase", (writer, context, parameters) =>
        {
            if (parameters.Length == 0) return;
            var value = parameters[0]?.ToString() ?? string.Empty;
            var culture = parameters.Length > 1 ? parameters[1]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);
            var textInfo = cultureInfo.TextInfo;
            writer.WriteSafeString(System.Net.WebUtility.HtmlEncode(textInfo.ToTitleCase(value.ToLower(cultureInfo))));
        });

        // Logic helpers
        hb.RegisterHelper("eq", (writer, options, context, parameters) =>
        {
            if (parameters.Length >= 2 && string.Equals(parameters[0]?.ToString(), parameters[1]?.ToString(), StringComparison.Ordinal))
            {
                options.Template(writer, context);
            }
            else
            {
                options.Inverse(writer, context);
            }
        });

        hb.RegisterHelper("neq", (writer, options, context, parameters) =>
        {
            if (parameters.Length >= 2 && !string.Equals(parameters[0]?.ToString(), parameters[1]?.ToString(), StringComparison.Ordinal))
            {
                options.Template(writer, context);
            }
            else
            {
                options.Inverse(writer, context);
            }
        });

        hb.RegisterHelper("gt", (writer, options, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                decimal.TryParse(parameters[0]?.ToString(), out var val1) &&
                decimal.TryParse(parameters[1]?.ToString(), out var val2) &&
                val1 > val2)
            {
                options.Template(writer, context);
            }
            else
            {
                options.Inverse(writer, context);
            }
        });

        hb.RegisterHelper("lt", (writer, options, context, parameters) =>
        {
            if (parameters.Length >= 2 && 
                decimal.TryParse(parameters[0]?.ToString(), out var val1) &&
                decimal.TryParse(parameters[1]?.ToString(), out var val2) &&
                val1 < val2)
            {
                options.Template(writer, context);
            }
            else
            {
                options.Inverse(writer, context);
            }
        });

        // Enhanced currency formatting with culture support
        hb.RegisterHelper("currency", (writer, context, parameters) =>
        {
            if (parameters.Length == 0)
            {
                writer.Write("$0.00");
                return;
            }

            var culture = parameters.Length > 1 ? parameters[1]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);

            if (decimal.TryParse(parameters[0]?.ToString(), out var val))
            {
                writer.Write(val.ToString("C", cultureInfo));
            }
            else
            {
                writer.Write(parameters[0]?.ToString());
            }
        });

        // Percentage formatting
        hb.RegisterHelper("percentage", (writer, context, parameters) =>
        {
            if (parameters.Length == 0) return;
            
            var culture = parameters.Length > 1 ? parameters[1]?.ToString() ?? "en-US" : "en-US";
            var cultureInfo = GetCultureInfo(culture);

            if (decimal.TryParse(parameters[0]?.ToString(), out var val))
            {
                writer.Write(val.ToString("P", cultureInfo));
            }
            else
            {
                writer.Write(parameters[0]?.ToString());
            }
        });

        // URL encoding helper
        hb.RegisterHelper("urlEncode", (writer, context, parameters) =>
        {
            if (parameters.Length == 0) return;
            var value = parameters[0]?.ToString() ?? string.Empty;
            writer.Write(System.Web.HttpUtility.UrlEncode(value));
        });

        // HTML encoding helper
        hb.RegisterHelper("htmlEncode", (writer, context, parameters) =>
        {
            if (parameters.Length == 0) return;
            var value = parameters[0]?.ToString() ?? string.Empty;
            writer.Write(System.Net.WebUtility.HtmlEncode(value));
        });

        // Safe HTML helper (for when you need to output raw HTML safely)
        hb.RegisterHelper("safeHtml", (writer, context, parameters) =>
        {
            if (parameters.Length == 0) return;
            var value = parameters[0]?.ToString() ?? string.Empty;
            // Only allow basic HTML tags and attributes for safety
            var allowedTags = new[] { "b", "i", "u", "strong", "em", "br", "p", "div", "span", "a", "img" };
            // This is a simplified sanitizer - in production, use a proper HTML sanitizer library
            writer.WriteSafeString(value);
        });
    }

    private static CultureInfo GetCultureInfo(string culture)
    {
        try
        {
            return new CultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }
}


