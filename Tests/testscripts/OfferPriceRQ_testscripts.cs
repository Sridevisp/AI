using System.Net.Http.Json;
using Xunit;
using System.Text.Json;
using System.Text;
using System.Net;

public class OfferPriceRQTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public OfferPriceRQTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task OfferPriceRQ_001_ValidSingleOfferRefID_ReturnsCorrectTotalPrice()
    {
        var request = new { OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001" };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("totalPrice", out var tp));
        Assert.True(tp.GetDecimal() > 0);
    }

    [Fact]
    public async Task OfferPriceRQ_002_ValidOfferRefIDWithMultipleOfferItemRefIDs_ReturnsCorrectTotalPrice()
    {
        var request = new
        {
            OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
            OfferItemRefIDs = new[] { "item1", "item2", "item3" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("totalPrice", out var tp));
        Assert.True(tp.GetDecimal() > 0);
    }

    [Fact]
    public async Task OfferPriceRQ_003_MissingOfferRefID_Returns400()
    {
        var response = await _client.PostAsync("/api/OfferPriceRQ", new StringContent("{}", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OfferPriceRQ_004_MalformedOfferRefID_Returns422()
    {
        var request = new { OfferRefID = "invalid-offer-id" };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task OfferPriceRQ_005_NonExistentOfferRefID_Returns404()
    {
        var request = new { OfferRefID = "00000000-0000-0000-0000-000000000000|xxxxxxxxxxxxxxxxxxxxxxxx" };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OfferPriceRQ_006_ExpiredOfferRefID_Returns410()
    {
        var request = new { OfferRefID = "expired-offer-id" };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request);
        Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
    }

    [Fact]
    public async Task OfferPriceRQ_007_ValidOfferRefIDWithPartialOfferItemRefIDs_ReturnsPriceForSelectedItems()
    {
        var request = new
        {
            OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
            OfferItemRefIDs = new[] { "item1" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("totalPrice", out var tp));
        Assert.True(tp.GetDecimal() > 0);
    }

    [Fact]
    public async Task OfferPriceRQ_008_InvalidOfferItemRefIDFormat_Returns422()
    {
        var request = new
        {
            OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
            OfferItemRefIDs = new[] { "invalid-item-id" }
        };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task OfferPriceRQ_009_ResponseIncludesCurrencyAndPriceBreakdown()
    {
        var request = new { OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001" };
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("currency", out var _));
        Assert.True(json.TryGetProperty("priceBreakdown", out var _));
    }

    [Fact]
    public async Task OfferPriceRQ_010_LargePayloadWith50OfferItemRefIDs_ReturnsSuccessfully()
    {
        var items = Enumerable.Range(1, 50).Select(i => $"item{i}").ToArray();
        var request = new
        {
            OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
            OfferItemRefIDs = items
        };
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var response = await _client.PostAsJsonAsync("/api/OfferPriceRQ", request, cts.Token);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("totalPrice", out var tp));
        Assert.True(tp.GetDecimal() > 0);
    }
}