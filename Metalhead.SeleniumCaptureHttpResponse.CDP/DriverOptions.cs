namespace Metalhead.SeleniumCaptureHttpResponse.CDP;

public class DriverOptions
{
    public const string DriverSettings = "Settings";

    public required string WebDriverPath { get; set; }
    public required string BrowserExecutableFullPath { get; set; }
}
