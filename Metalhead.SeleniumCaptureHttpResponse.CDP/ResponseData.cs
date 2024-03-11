namespace Metalhead.SeleniumCaptureHttpResponse.CDP;

public class ResponseData(string url)
{
    public string Url { get; set; } = url;
    public string? Body { get; set; } = null;
    public bool CaptureSuccess { get; set; } = false;
}
