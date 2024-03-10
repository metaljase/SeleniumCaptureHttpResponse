using Fetch = OpenQA.Selenium.DevTools.V122.Fetch;

namespace Metalhead.SeleniumCDT.CaptureHttpResponse
{
    public class Response(Fetch.RequestPausedEventArgs requestPausedEventArgs, Fetch.GetResponseBodyCommandResponse getResponseBodyCommandResponse)
    {
        public Fetch.RequestPausedEventArgs? RequestPausedEventArgs { get; set; } = requestPausedEventArgs;
        public Fetch.GetResponseBodyCommandResponse? GetResponseBodyCommandResponse { get; set; } = getResponseBodyCommandResponse;

        public override string ToString()
        {
            if (GetResponseBodyCommandResponse != null)
            {
                var body = GetResponseBodyCommandResponse.Body;
                if (GetResponseBodyCommandResponse.Base64Encoded)
                {
                    body = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(body));
                }
                return body;
            }

            return string.Empty;
        }
    }
}
