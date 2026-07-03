using Microsoft.Playwright;

namespace CarvedRock.End2End.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ApiMockTestWithVideo : PageTest
{
    private string _baseUrl = null!;
    private IPage _page = null!;
    private IBrowserContext _browserContext = null!;

    [OneTimeSetUp] 
    public async Task SetUp()
    {
        _baseUrl = Utilities.GetBaseUrl();
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(); // or WebKit, Firefox

        _browserContext = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            RecordVideoDir = "videos/"
            });
        
        _page = await _browserContext.NewPageAsync();
        await _browserContext.Tracing.StartAsync(new()
        {
            Title = "ApiMockTraces",
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }

    [Test]
    public async Task MockedItemsOnFootwearPage()
    {
        await _page.GotoAsync(_baseUrl);

        await _page.RouteAsync("*/**/Product?category=boots", async route =>
        {
            await route.FulfillAsync(new() { Path = "boots-mock.json" });
        });
        await _page.GetByRole(AriaRole.Link, new() { Name = "Footwear" }).ClickAsync();
        await Expect(_page.GetByAltText("Coastliner")).ToBeVisibleAsync();
        
        // mocked api is not available from server-side calls
        await Expect(_page.GetByAltText("MOCKED-Coastliner")).ToBeHiddenAsync();
    }

    [OneTimeTearDown]
    public async Task StopTracing()
    {
        await _page.Context.Tracing.StopAsync(new()
        {
            Path = Path.Combine(
                Environment.CurrentDirectory,
                "playwright-traces",
                "api-mock-traces-with-video.zip"
            )
        });
        await _page.Context.CloseAsync();
    }
}
