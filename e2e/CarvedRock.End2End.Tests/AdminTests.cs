using Bogus;
using Microsoft.Playwright;

namespace CarvedRock.End2End.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AdminTests : PageTest
{
    private const string _authenticationStateFilename = "authState.json";
    private IPage _page = null!;
    private string _baseUrl = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Randomizer.Seed = new Random(Guid.NewGuid().GetHashCode());
        _baseUrl = Utilities.GetBaseUrl();
    }

    //[OneTimeSetup] // happens once before all tests in this class
    [SetUp] // happens before each test method in this class
    public async Task Setup()
    {
        if (!File.Exists(_authenticationStateFilename))
        {
            await LoginAsAnAdmin();
        }

        //await LoginAsAnAdmin();        
        var browserContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = _authenticationStateFilename
        });
        _page = await browserContext.NewPageAsync();
    }

    [OneTimeTearDown] // done after all tests in this class
    public void OneTimeTearDown()
    {
        File.Delete(_authenticationStateFilename);
    }

    [Test]
    public async Task AdminIsAvailableForBob()
    {
        //await LoginAsAnAdmin();
        await _page.GotoAsync(_baseUrl);
        //await Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" }).ClickAsync();
        //await Page.GetByPlaceholder("Username").FillAsync("bob");
        //await Page.GetByPlaceholder("Username").PressAsync("Tab");
        //await Page.GetByPlaceholder("Password").FillAsync("bob");        
        //await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Admin" })).ToBeVisibleAsync();

        await _page.GetByRole(AriaRole.Link, new() { Name = "Admin" }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Create New" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task ValidationErrorsAppearOnEmptySubmission()
    {
        //await LoginAsAnAdmin();
        await _page.GotoAsync($"{_baseUrl}/admin");
        //await Page.GetByPlaceholder("Username").FillAsync("bob");
        //await Page.GetByPlaceholder("Username").PressAsync("Tab");
        //await Page.GetByPlaceholder("Password").FillAsync("bob");
        //await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await _page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();

        // TODO: Anything to check on the initial state of the "create product" form?

        await _page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        await Expect(_page.Locator("form")).ToContainTextAsync("The Name field is required.");

        // TODO: More validation error checks -- maybe even a data driven test using TestCaseSource?
    }

    [Test]
    public async Task CanSubmitNewProductSuccessfully()
    {
        // form interactions: https://playwright.dev/dotnet/docs/input

        //await LoginAsAnAdmin();
        await _page.GotoAsync($"{_baseUrl}/admin");
        //await Page.GetByPlaceholder("Username").FillAsync("bob");
        //await Page.GetByPlaceholder("Username").PressAsync("Tab");
        //await Page.GetByPlaceholder("Password").FillAsync("bob");
        //await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await _page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();

        // Creating new product now

        var categories = new List<string> { "boots", "equip", "kayak" };
        var priceRanges = new Dictionary<string, List<decimal>>
        {
            { "boots", [50, 300] },
            { "kayak", [100, 500] },
            { "equip", [20, 150] }
        };
        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.Product())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Category, f => f.PickRandom(categories))
            .RuleFor(p => p.Price, (f, p) => double.Parse(f.Commerce.Price(priceRanges[p.Category][0],
                                                priceRanges[p.Category][1]))) // inside valid price range for all categories            
            .RuleFor(p => p.ImgUrl, f => f.Image.PicsumUrl());
        var productToCreate = productFaker.Generate(); //(25); // if you need to generate many

        // check for initial focus??
        await _page.GetByLabel("Name").ClickAsync();
        await _page.GetByLabel("Name").FillAsync(productToCreate.Name);
        await _page.GetByLabel("Category").ClickAsync();  // navigate with mouse
        await _page.GetByLabel("Category").FillAsync(productToCreate.Category);
        await _page.GetByLabel("Price").ClickAsync();
        await _page.GetByLabel("Price").FillAsync($"{productToCreate.Price}");
        await _page.GetByLabel("Price").PressAsync("Tab"); // navigate with keyboard
        await _page.GetByLabel("Description").FillAsync(productToCreate.Description);
        await _page.GetByLabel("Description").PressAsync("Tab");
        await _page.GetByLabel("ImgUrl").FillAsync(productToCreate.ImgUrl);

        await _page.ScreenshotAsync(new() { Path = "screenshot-create-filled-in.png" });

        await _page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

        await _page.ScreenshotAsync(new() { Path = "screenshot-new-admin-list.png" });

        await Expect(_page.Locator("tbody")).ToContainTextAsync(productToCreate.Name);
        await Expect(_page.Locator("tbody")).ToContainTextAsync(productToCreate.Description);
        await Expect(_page.Locator("tbody")).ToContainTextAsync(productToCreate.Category);
        await Expect(_page.Locator("tbody")).ToContainTextAsync($"{productToCreate.Price}");

        TestContext.Out.WriteLine($"Created new product {productToCreate.Name} of type {productToCreate.Category}.");

        //BETTER: 
        //await Expect(Page.Locator("tbody")).ToContainTextAsync($"{productToCreate.Price:F2}");

        //ALSO BETTER: Find specific row for new product and validate the contents in that row
    }

    [Test]
    public async Task CanDeleteProduct()
    {
        await LoginAsAnAdmin();
        await _page.GotoAsync($"{_baseUrl}/admin");
        //await Page.GetByPlaceholder("Username").FillAsync("bob");
        //await Page.GetByPlaceholder("Username").PressAsync("Tab");
        //await Page.GetByPlaceholder("Password").FillAsync("bob");
        //await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        // Dialogs: https://playwright.dev/dotnet/docs/dialogs
        void Page_Dialog_EventHandler(object sender, IDialog dialog)
        {
            Console.WriteLine($"Dialog message: {dialog.Message}");
            dialog.DismissAsync();
            _page.Dialog -= Page_Dialog_EventHandler!;
        }
        _page.Dialog += Page_Dialog_EventHandler!;

        // original code (no data-testid attribute on the row)
        //await Page.Locator("tr:nth-child(2) > td:nth-child(6)").ClickAsync();

        var deleteLink = _page.GetByTestId("Coastliner").GetByRole(AriaRole.Link, new() { Name = "Delete" });
        await deleteLink.ClickAsync();

        await Expect(_page.GetByLabel("Confirm Delete")).ToContainTextAsync(new Regex("delete.*Coastliner"));

        await _page.GetByLabel("Confirm Delete").GetByText("Delete", new() { Exact = true }).ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Heading, new() { Name = "Congratulations!" })).ToBeVisibleAsync();

        await Expect(_page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("You have successfully (not really) deleted the product ID [2]");
    }

    private async Task LoginAsAnAdmin()
    {
        await Page.GotoAsync(_baseUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Sign in" }).ClickAsync();
        await Page.GetByPlaceholder("Username").FillAsync("bob");
        await Page.GetByPlaceholder("Username").PressAsync("Tab");
        await Page.GetByPlaceholder("Password").FillAsync("bob");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        Console.WriteLine("Logged in as bob - an admin");

        // THIS LINE IS IMPORTANT!! -- you need to get BACK to the site in order to
        // create finish the login and create the cookie that will auth in the CR site.
        await Expect(Page.Locator("#navbarNav")).ToContainTextAsync("BobSmith@email.com");

        // Save the authentication state to a file
        await Page.Context.StorageStateAsync(new() { Path = _authenticationStateFilename });
    }
}

public record Product
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public double Price { get; set; }
    public string Category { get; set; } = null!;
    public string ImgUrl { get; set; } = null!;
}