# What's SeleniumCaptureHttpResponse?
SeleniumCaptureHttpResponse is an example of how to capture an HTTP response using Selenium WebDriver, ChromeDriver, Chrome DevTools Protocol (CDP), and C# (.NET 6). It is not a standalone application, it's a working example, of how you can capture HTTP responses (e.g. JSON from API calls) on web pages, inc. 3rd party websites.  Therefore, you'll need software to compile the code, e.g. Visual Studio 2022.

## What's Selenium WebDriver?
[Selenium](https://www.selenium.dev) WebDriver controls compatible web browsers (inc. Firefox, Chrome, Edge, Internet Explorer) natively, as a real user would.

## What's ChromeDriver?
[ChromeDriver](https://sites.google.com/chromium.org/driver/) is an executable that Selenium WebDriver uses to control Chrome.

## What's Chrome DevTools Protocol?
[Chrome DevTools Protocol](https://chromedevtools.github.io/devtools-protocol/) (CDP) allows for tools to instrument, inspect, debug, and profile, Chromium, Chrome, and other Blink-based browsers.  SeleniumCaptureHttpResponse uses CDP to capture HTTP responses.

## Where could this be useful?
Testing a web page for bugs or as part of scraping a web page where capturing HTTP traffic is desired.

## Setup instructions
1. Install the Chrome web browser.
2. Download the version of [ChromeDriver](https://chromedriver.storage.googleapis.com/index.html) that matches your version of Chrome. Note: 32-bit version works with 64-bit Chrome.
3. Clone the SeleniumCaptureHttpResponse repository.
4. Open the .NET solution in Visual Studio 2022 (or a compatible alternative).
5. Open the `appsettings.json` file and edit the paths to match the location of Chrome and ChromeDriver on your PC.
6. Build the solution and run!

This should open Chrome and automatically visit [my example web page](https://metaljase.github.io/SeleniumCaptureHttpResponse.html).  The JavaScript on that web page calls a [3rd party API](https://jsonplaceholder.typicode.com/users/) and returns data as JSON in the HTTP response.
The code in `Program.cs` captures the HTTP response, stores the JSON from the message body, then outputs the JSON in the console window.  In a real use case you'll probably do something more exciting with the HTTP response!
