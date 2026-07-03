using Microsoft.Playwright;

namespace CarvedRock.End2End.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ApiMockTests : PageTest
{
    private string _baseUrl = null!;

    [SetUp]
    public async Task SetupTracing()
    {
        _baseUrl = Utilities.GetBaseUrl();
        await Context.Tracing.StartAsync(new()
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
        await Page.GotoAsync(_baseUrl);

        await Page.RouteAsync("*/**/Product?category=boots", async route =>
        {
            await route.FulfillAsync(new() { Path = "boots-mock.json" });
        });
        await Page.GetByRole(AriaRole.Link, new() { Name = "Kayaks" }).ClickAsync();
        await Expect(Page.GetByAltText("Glide")).ToBeVisibleAsync(); // from kayaks list
    }

    [TearDown]
    public async Task StopTracing()
    {
        await Context.Tracing.StopAsync(new()
        {
            Path = Path.Combine(
                Environment.CurrentDirectory,
                "playwright-traces",
                "api-mock-traces.zip"
            )
        });
    }
}
