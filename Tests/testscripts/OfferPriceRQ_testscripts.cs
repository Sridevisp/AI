using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class OfferPriceRQTests
{
    private readonly HttpClient _client = new HttpClient();
    private const string Endpoint = "https://api.example.com/v1/OfferPriceRQ";

    [Fact]
    public async Task OfferPriceRQ_001_ValidSingleOffer_ReturnsTotalPrice()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-1", PaxRefID = "ADULT_1" },
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-2", PaxRefID = "YOUNG_1" },
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-3", PaxRefID = "CHILD_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PricedOffer", content);
        Assert.Contains("TotalPrice", content);
    }

    [Fact]
    public async Task OfferPriceRQ_002_MultipleValidOffers_ReturnsAggregatedPrice()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-1", PaxRefID = "ADULT_1" }
                    }
                },
                new
                {
                    OfferRefID = "b2716059-ee3b-487c-8a63-d72a8c7810a8|am9f9wivlT9SV0Wd7zNRI7002",
                    OwnerCode = "BA",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "b2716059-ee3b-487c-8a63-d72a8c7810a8|am9f9wivlT9SV0Wd7zNRI7002-1", PaxRefID = "ADULT_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PricedOffer", content);
    }

    [Fact]
    public async Task OfferPriceRQ_003_EmptySelectedOfferList_Returns400()
    {
        var payload = new { SelectedOfferList = new object[] { } };
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("SelectedOfferList cannot be empty", content);
    }

    [Fact]
    public async Task OfferPriceRQ_004_MissingOfferRefID_Returns400()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-1", PaxRefID = "ADULT_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("OfferRefID is required", content);
    }

    [Fact]
    public async Task OfferPriceRQ_005_InvalidOfferRefIDFormat_Returns400()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "invalid-uuid-format",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "invalid-uuid-format-1", PaxRefID = "ADULT_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("invalid OfferRefID format", content);
    }

    [Fact]
    public async Task OfferPriceRQ_006_MissingOwnerCode_Returns400()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-1", PaxRefID = "ADULT_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("OwnerCode is required", content);
    }

    [Fact]
    public async Task OfferPriceRQ_007_EmptySelectedOfferItems_Returns400()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new object[] { }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("at least one SelectedOfferItem is required", content);
    }

    [Fact]
    public async Task OfferPriceRQ_008_MismatchedOfferItemRefID_Returns400()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "different-uuid|different-suffix-1", PaxRefID = "ADULT_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("OfferItemRefID must match OfferRefID prefix", content);
    }

    [Fact]
    public async Task OfferPriceRQ_009_DuplicatePaxRefID_Returns400()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-1", PaxRefID = "ADULT_1" },
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-2", PaxRefID = "ADULT_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("duplicate passenger references", content);
    }

    [Fact]
    public async Task OfferPriceRQ_010_NonExistentOfferRefID_Returns404()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "00000000-0000-0000-0000-000000000000|nonexistent",
                    OwnerCode = "VS",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "00000000-0000-0000-0000-000000000000|nonexistent-1", PaxRefID = "ADULT_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("offer not found", content);
    }

    [Fact]
    public async Task OfferPriceRQ_011_SpecialCharactersInOwnerCode_Returns200()
    {
        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "V&S-123",
                    SelectedOfferItems = new[]
                    {
                        new { OfferItemRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-1", PaxRefID = "ADULT_1" }
                    }
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PricedOffer", content);
    }

    [Fact]
    public async Task OfferPriceRQ_012_MaximumSelectedOfferItems_Returns200()
    {
        var offerItems = new object[9];
        for (int i = 0; i < 9; i++)
        {
            offerItems[i] = new
            {
                OfferItemRefID = $"a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001-{i + 1}",
                PaxRefID = $"ADULT_{i + 1}"
            };
        }

        var payload = new
        {
            SelectedOfferList = new[]
            {
                new
                {
                    OfferRefID = "a2716059-ee3b-487c-8a63-d72a8c7810a7|am9f9wivlT9SV0Wd7zNRI7001",
                    OwnerCode = "VS",
                    SelectedOfferItems = offerItems
                }
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await _client.PostAsync(Endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("PricedOffer", content);
    }
}