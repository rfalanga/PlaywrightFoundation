# wright End-to-End Tests

This solution / project is a collection of end-to-end tests
using the [playwright](https://playwright.dev/) framework and
the NUnit testing framework.

## Getting Started

To run these tests, the Aspire-based solution in
the root folder of this repository should be running.
Follow the instructions in [that readme](../readme.md).

You may also need to install the browsers needed
by Playwright.  

First build the solution.

Then run the following command from a terminal:

```bash
./CarvedRock.End2End.Tests/bin/Debug/net9.0/playwright.ps1 install
```

Then you should be able to run the tests!

## Using the Recorder

An easy way to create tests is using the Playwright recorder:

```bash
./CarvedRock.End2End.Tests/bin/Debug/net9.0/playwright.ps1 codegen https://localhost:7224
```

You can also install the [Playwright CRX](https://chromewebstore.google.com/detail/playwright-crx/jambeljnbnfbkcpnoiaedcabbgmnnlcd) extension from
the Chrome Web Store to have it "always available"
without needing to run the above command.

## Test Features and Notes

* If you have a `<table>` including a `data-testid` attribute for
each `<tr>` element you can use the `GetByTestId` locator which will help
improve the accuracy / resilience of tests.
* Shared authentication context (see `AdminTests.cs`) can improve performance when
you need to be logged in to the application for different tests.
* Screenshots, traces, videos, and `TestContext.Out.WriteLine` are all good
ways to help troubleshoot tests and examples of each are included.
* API tests can be written with Playwright (see `ApiTests.cs`)
* API responses can be mocked (but only browser-initiated API calls).  See `ApiMockTests.cs`.
* Complete "manual setup" is possible - see `ApiMockTestWithVideo.cs`.
* Parallelism happens across test classes, but within a class each test is run one at a time.
* Custom timeouts can be applied when using locators -- see `FirstTests.cs`->`DelayedContentShowsUp()`.

## Running Tests

### Use Your IDE

You can run the tests using your IDE's test runner.  This will work in Visual Studio, Rider, or VS Code 
with the [C# DevKit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) installed.

### Use the Command Line

You can run the tests using the .NET CLI.  This will work in any terminal with the .NET SDK installed.

```bash
dotnet test
```
To target a specific browser, use `chromium`, `firefox`, or `webkit` as an argument to dotnet test as shown below:

```bash
dotnet test -- wright:browser=chromium
```

#### Parameters from the Command Line

To specify parameters from the command line, use the following syntax:

```bash
# cmd
dotnet test  -- TestRunParameters.Parameter(name=\"myParam\", value=\"value\")

# powershell
dotnet test --%  -- TestRunParameters.Parameter(name=\"myParam\", value=\"value\") 

# bash
dotnet test -- TestRunParameters.Parameter\(name=\"myParam\",\ value=\"value\"\) 
```

Since I'm using PowerShell, this is a sample command to test
against remote URLs (you would need to provide your own URLs if you
deploy the app somewhere):

```bash
dotnet test --% -- TestRunParameters.Parameter(name=\"BaseUrl\", value=\"https://carvedrock-webapp.whiteglacier-d72dac78.eastus2.azurecontainerapps.io\") TestRunParameters.Parameter(name=\"ApiUrl\", value=\"https://carvedrock-api.whiteglacier-d72dac78.eastus2.azurecontainerapps.io\") wright.BrowserName=webkit
```

## Using `.runsettings` Files

A file specifically named `.runsettings` in the solution
root can be automatically picked up by test runners in IDEs.

From the command line, you can specify the runsettings file as follows:

```bash
dotnet test -s <filename>
```

You could put the remote URLs and/or browsers into a
runsettings file to simplify the command line - see
`aca-firefox.runsettings` as an example.
