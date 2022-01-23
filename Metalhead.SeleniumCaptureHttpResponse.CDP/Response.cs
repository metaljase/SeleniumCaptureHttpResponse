using Fetch = OpenQA.Selenium.DevTools.V96.Fetch;

namespace Metalhead.SeleniumCDT.CaptureHttpResponse
{
    public class Response
    {
        public Response(Fetch.RequestPausedEventArgs requestPausedEventArgs, Fetch.GetResponseBodyCommandResponse getResponseBodyCommandResponse)
        {
            RequestPausedEventArgs = requestPausedEventArgs;
            GetResponseBodyCommandResponse = getResponseBodyCommandResponse;
        }

        public Fetch.RequestPausedEventArgs? RequestPausedEventArgs { get; set; }
        public Fetch.GetResponseBodyCommandResponse? GetResponseBodyCommandResponse { get; set; }

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

            return String.Empty;
        }
    }
}
