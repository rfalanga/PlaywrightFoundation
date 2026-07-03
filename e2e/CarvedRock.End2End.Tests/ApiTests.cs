using Microsoft.Playwright;
using System.Text.Json;

namespace CarvedRock.End2End.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ApiTests : PlaywrightTest
{
    private IAPIRequestContext _request = null!;

    private readonly JsonSerializerOptions _jsonOptions = 
        new() { PropertyNameCaseInsensitive = true };

    [SetUp]
    public async Task Setup()
    {
        _request = await Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = Utilities.GetApiUrl(),
            IgnoreHTTPSErrors = true // Ignore self-signed certificates (if localhost only?)
        });
        // add headers, get auth token, do OneTimeSetup, etc
    }

    [Test]
    public async Task GetProductsReturnsInitialProducts()
    {
        var productsResponse = await _request.GetAsync("/product?category=all");
        await Expect(productsResponse).ToBeOKAsync(); // playwright assertion
        
        var productsJson = await productsResponse.TextAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(productsJson, _jsonOptions);

        var expectedNames = new[] { "Trailblazer", "Sherpa", "Coastliner", 
                "Woodsman", "Billy", "Glide" };

        foreach (var expectedName in expectedNames)
        {
            // could get fancier here checking more properties, etc
            var product = products!.FirstOrDefault(p => p.Name == expectedName);
            Assert.That(product, Is.Not.Null); // NUnit assertion
        }
    }
    
}
