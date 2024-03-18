using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Metalhead.SeleniumCaptureHttpResponse.CDP;

public class DriverOptionsValidation(IConfiguration config) : IValidateOptions<DriverOptions>
{
    public DriverOptions? Config { get; private set; } = config.GetSection(DriverOptions.DriverSettings).Get<DriverOptions>();

    public ValidateOptionsResult Validate(string? name, DriverOptions options)
    {
        List<ValidationResult> validationResults = [];

        if (!string.IsNullOrWhiteSpace(options.WebDriverPath))
        {
            var webDriverPath = Environment.ExpandEnvironmentVariables(options.WebDriverPath);
            if (webDriverPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                validationResults.Add(new ValidationResult($"{nameof(DriverOptions.WebDriverPath)} path in app settings ('{options.WebDriverPath}') contains invalid characters."));
            }
            else if (!Path.Exists(webDriverPath))
            {
                validationResults.Add(new ValidationResult($"{nameof(DriverOptions.WebDriverPath)} full path to Chrome driver executable in app settings ('{options.WebDriverPath}') does not exist."));
            }
        }

        if (!string.IsNullOrWhiteSpace(options.BrowserExecutableFullPath))
        {
            var browserExecutableFullPath = Environment.ExpandEnvironmentVariables(options.BrowserExecutableFullPath);
            if (browserExecutableFullPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                validationResults.Add(new ValidationResult($"{nameof(DriverOptions.BrowserExecutableFullPath)} path in app settings ('{options.BrowserExecutableFullPath}') contains invalid characters."));
            }
            else if (!File.Exists(browserExecutableFullPath))
            {
                validationResults.Add(new ValidationResult($"{nameof(DriverOptions.BrowserExecutableFullPath)} path in app settings ('{options.BrowserExecutableFullPath}') does not exist."));
            }
        }

        if (validationResults.Count > 0)
        {
            var failures = validationResults.Where(v => v.ErrorMessage is not null).Select(v => v.ErrorMessage!);
            return ValidateOptionsResult.Fail(failures);
        }

        return ValidateOptionsResult.Success;
    }
}
