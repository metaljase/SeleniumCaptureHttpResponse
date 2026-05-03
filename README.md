# What's SeleniumCaptureHttpResponse?
SeleniumCaptureHttpResponse contains two C# .NET 8 console apps that demonstrate how to capture a HTTP responses using Selenium WebDriver, ChromeDriver, and Chrome DevTools Protocol (CDP).  It is not a standalone application, it's a working example of how you can capture HTTP responses (e.g. JSON from API calls) on web pages, inc. 3rd party websites.  Therefore, you'll need software to compile the code, e.g. Visual Studio 2026.

## What's Selenium WebDriver?
[Selenium](https://www.selenium.dev) WebDriver controls compatible web browsers (inc. Firefox, Chrome, Edge, Internet Explorer) natively, as a real user would.  [Selenium .NET API documentation](https://www.selenium.dev/selenium/docs/api/dotnet/index.html) can be found on their website.

## What's ChromeDriver?
[ChromeDriver](https://sites.google.com/chromium.org/driver/) is an executable that Selenium WebDriver uses to control Chrome.

## What's Chrome DevTools Protocol?
[Chrome DevTools Protocol](https://chromedevtools.github.io/devtools-protocol/) (CDP) allows for tools to instrument, inspect, debug, and profile, Chromium, Chrome, and other Blink-based browsers.  SeleniumCaptureHttpResponse uses CDP to capture HTTP responses.

## Where could this be useful?
Testing a web page for bugs or as part of scraping a web page where capturing HTTP traffic is desired.

## What's the difference between the two console apps?
`Metalhead.SeleniumCaptureHttpResponse.Selenium4` uses Selenium 4 to capture HTTP responses.
`Metalhead.SeleniumCaptureHttpResponse.CDP` uses Selenium and Chrome DevTools Protocol to capture HTTP responses.  CDP gives you more control than Selenium 4, but it is also more complex to set up and use.

## Setup instructions
1. Install the Chrome web browser.
2. Download the version of ChromeDriver that matches your version of Chrome.  If don't need the [latest version of ChromeDriver](https://googlechromelabs.github.io/chrome-for-testing/), then [older versions](https://chromedriver.chromium.org/downloads) are available.  Note: 32-bit version works with 64-bit Chrome.
3. Clone the SeleniumCaptureHttpResponse repository.
4. Open the .NET solution in Visual Studio 2026 (or a compatible alternative).
5. Open the `appsettings.json` file and edit the paths to match the location of Chrome and ChromeDriver on your PC.  Note: If you set these paths to `null`, then the code will use the default locations for Chrome and ChromeDriver.
6. Build the solution and run!

This should open Chrome and automatically visit [my example web page](https://metaljase.github.io/SeleniumCaptureHttpResponse.html).  The JavaScript in that web page makes two API calls, returning [users](https://jsonplaceholder.typicode.com/users/) and [albums](https://jsonplaceholder.typicode.com/albums/) data as JSON in the HTTP responses.
The code in `CaptureResponse.cs` captures the HTTP responses, stores the JSON from the message body, then outputs the JSON in the console window.  In a real use case, you'll probably do something more exciting with the HTTP response!
