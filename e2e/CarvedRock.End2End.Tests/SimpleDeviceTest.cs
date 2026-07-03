using Microsoft.Playwright;
namespace CarvedRock.End2End.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class SimpleDeviceTest : PlaywrightTest
{
    private string _baseUrl = null!;
    [SetUp]
    public void Setup() 
    {
        _baseUrl = Utilities.GetBaseUrl();
    }

    // Devices and their settings:
    // https://github.com/microsoft/playwright/blob/main/packages/playwright-core/src/server/deviceDescriptorsSource.json
    [TestCase("iPhone 14")]
    [TestCase("iPhone 15 Pro Max")]
    public async Task Test(string deviceName)
    {
        var browser = await Playwright.Chromium.LaunchAsync();
        var device = Playwright.Devices[deviceName];
        var context = await browser.NewContextAsync(device);
        
        var devicePage = await context.NewPageAsync();
        await devicePage.GotoAsync(_baseUrl);
        var bannerTextLocator = devicePage.GetByText("GET A GRIP");

        await devicePage.ScreenshotAsync(new PageScreenshotOptions 
        {
            Path = $"screenshot-{deviceName}.png" 
        });
        await browser.CloseAsync();
    }
}
