using Microsoft.Playwright;

namespace CarvedRock.End2End.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    internal string _baseUrl = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _baseUrl = Utilities.GetBaseUrl();
    }

    [SetUp]
    public void Setup()
    {
        TestContext.Out.WriteLine($"Using base URL: {_baseUrl}");
    }

    [Test]
    public async Task HomePageHasCorrectContent()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.ScreenshotAsync(new() { Path = "screenshot.png" });

        // Expect a title "to contain" a substring.
        await Expect(Page).ToHaveTitleAsync("Carved Rock Fitness");

        var bannerTextLocator = Page.GetByText("GET A GRIP");
        await bannerTextLocator.ScreenshotAsync(new() { Path = "screenshotOfLocator.png" });

        await bannerTextLocator.HighlightAsync(); // troubleshooting only - don't commit
        await Page.ScreenshotAsync(new() { Path = "screenshotWithHighlight.png" });

        await Expect(bannerTextLocator).ToBeVisibleAsync();

        // https://playwright.dev/dotnet/docs/aria-snapshots
        //await Expect(Page.Locator("#navbarNav")).ToMatchAriaSnapshotAsync("- list:\n  - listitem:\n    - link \"Footwear\"\n  - listitem:\n    - link \"Kayaks\"\n  - listitem:\n    - link \"Equipment\"\n  - listitem:\n    - link \"Cart (3)\"\n- list:\n  - listitem:\n    - link \"IdSrv\"\n- list:\n  - listitem:\n    - link \"Sign in\"");            
        await Expect(Page.Locator("#navbarNav")).ToMatchAriaSnapshotAsync(@"
                - list:  
                    - listitem:
                        - link ""Footwear""
                    - listitem:
                        - link ""Kayaks""
                    - listitem: 
                        - link ""Equipment""
                    - listitem:  
                        - link /Cart \(\d+\)/
                - list:
                    - listitem:
                        - link ""IdSrv""
                - list:
                    - listitem:
                        - link ""Sign in"" ");

        //await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync("- heading \"GET A GRIP\" [level=1]\n- heading /\\d+% OFF/ [level=2]\n- paragraph: THROUGHOUT THE SEASON");
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(@"
                - heading ""GET A GRIP"" [level=1]
                - heading /\d+% OFF/ [level=2]
                - paragraph: THROUGHOUT THE SEASON");
    }

    [Test]
    public async Task CanAddItemsToCartOnFootwearPage()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Footwear" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Img, new() { Name = "Trailblazer" })).ToBeVisibleAsync();

        var btn1 = Page.Locator("#add-btn-1");
        await Expect(btn1).ToBeVisibleAsync();
        await btn1.ClickAsync();

        await Expect(Page.Locator("#carvedrockcart")).ToContainTextAsync("Cart (1)");
    }

    [TestCaseSource(nameof(Users))]
    public async Task AddItemsToCartAndVerifyContents(User user)
    {
        await Page.GotoAsync($"{_baseUrl}/Listing?cat=boots");
        var btn2 = Page.Locator("#add-btn-2");
        await btn2.ClickAsync();
        await btn2.ClickAsync();

        await Page.Locator("#add-btn-3").ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Kayaks" }).ClickAsync();
        await Page.GetByTestId("Glide").GetByRole(AriaRole.Button, new() { Name = "Add to Cart" }).ClickAsync();

        await Page.GetByRole(AriaRole.Link, new() { NameString = "Cart" }).ClickAsync();
        //await Page.GetByPlaceholder("Username").FillAsync("bob");
        //await Page.GetByPlaceholder("Username").PressAsync("Tab");
        var usernameField = Page.GetByPlaceholder("Username");
        await usernameField.FillAsync(user.Username);
        await usernameField.PressAsync("Tab");

        //await Page.GetByPlaceholder("Password").FillAsync("bob");
        //await Page.GetByPlaceholder("Password").PressAsync("Tab");
        var passwordField = Page.GetByPlaceholder("Password");
        await passwordField.FillAsync(user.Password);
        await passwordField.PressAsync("Tab");

        await Page.GetByLabel("Remember My Login").PressAsync("Tab");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        //await Expect(Page.Locator("tbody")).ToContainTextAsync("1");

        var table = Page.Locator("table");
        var coastlinerRow = table.Locator("tr").Filter(new() { HasText = "Coastliner" });
        await Expect(coastlinerRow).ToBeVisibleAsync();
        var quantityCell = coastlinerRow.Locator("td").Nth(3);
        await Expect(quantityCell).ToHaveTextAsync("2");

        var tableRows = table.Locator("tr");
        await Expect(tableRows).ToHaveCountAsync(6); // three products plus grand total plus header plus buttons in footer

        // exercise for you: create a parameterized test and calculate this
        // value based on known prices and the quantities of products you add
        // to the cart
        await Expect(Page.Locator("#grand-total")).ToContainTextAsync("$564.96");
    }

    [Test]
    public async Task DelayedContentShowsUp()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Footwear" }).ClickAsync();
        await Expect(Page.Locator("#content-with-delay"))
            .ToContainTextAsync("This content was delayed by 2000 milliseconds",
            new LocatorAssertionsToContainTextOptions
            {
                Timeout = 4_000
            });
    }

    public static User[] Users =
    [
        new("bob", "bob"),
            new("alice", "alice")
    ];
}

public record User(string Username, string Password);
