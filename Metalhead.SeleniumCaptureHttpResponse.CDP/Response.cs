using Fetch = OpenQA.Selenium.DevTools.V147.Fetch;

namespace Metalhead.SeleniumCaptureHttpResponse.CDP;

public class Response(Fetch.RequestPausedEventArgs requestPausedEventArgs, Fetch.GetResponseBodyCommandResponse getResponseBodyCommandResponse)
{
    public Fetch.RequestPausedEventArgs? RequestPausedEventArgs { get; set; } = requestPausedEventArgs;
    public Fetch.GetResponseBodyCommandResponse? GetResponseBodyCommandResponse { get; set; } = getResponseBodyCommandResponse;

    public override string ToString()
    {
        if (GetResponseBodyCommandResponse is not null)
        {
            var body = GetResponseBodyCommandResponse.Body;
            if (GetResponseBodyCommandResponse.Base64Encoded)
                body = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(body));

            return body;
        }

        return string.Empty;
    }
}
