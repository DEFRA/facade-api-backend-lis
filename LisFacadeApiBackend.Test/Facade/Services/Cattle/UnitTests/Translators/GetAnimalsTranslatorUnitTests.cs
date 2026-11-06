using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.LIS.Response;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.Translators;

public class GetAnimalsTranslatorUnitTests
{
    private const string SiteIdentifier = "12/345/6789";
    private const string County = "12";
    private const string Parish = "345";
    private const string Holding = "6789";

    private readonly ICattleApiClient _client = Substitute.For<ICattleApiClient>();
    private readonly GetAnimalsTranslator _translator;

    public GetAnimalsTranslatorUnitTests()
    {
        var service = new GetAnimalsService(_client, NullLogger<GetAnimalsService>.Instance);
        _translator = new GetAnimalsTranslator(service, NullLogger<GetAnimalsTranslator>.Instance);
    }

    private static CattleResponse Cattle(
        string earTag,
        string? sex = "female",
        string status = "registered",
        string? breedCode = null,
        string? breedName = null) => new()
    {
        EarTag = earTag,
        Sex = sex,
        Status = status,
        BreedCode = breedCode,
        BreedName = breedName
    };

    private static GetAnimalsRequest Request(
        int page = 1,
        int? pageSize = null,
        AnimalOrder order = AnimalOrder.AnimalIdentifier,
        string sort = "asc",
        Species species = Species.Cattle,
        string siteIdentifier = SiteIdentifier) => new()
    {
        Species = species,
        SiteIdentifier = siteIdentifier,
        Page = page,
        PageSize = pageSize,
        Order = order,
        Sort = sort
    };

    private void SetupCattle(params CattleResponse[] cattle) =>
        _client.GetCattleForHoldingMultiSegmentAsync(County, Parish, Holding, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(cattle.ToList()) });

    private static string[] Ids(GetAnimalsResponse response) =>
        response.Values!.Select(v => v.Animal!.Identifier).ToArray();

    [Fact]
    public async Task Translate_returns_empty_result_without_calling_api_for_non_cattle_species()
    {
        var result = await _translator.Translate(Request(species: Species.Sheep), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(0, result.Count);
        Assert.Empty(result.Values!);
        Assert.Empty(_client.ReceivedCalls());
    }

    [Theory]
    [InlineData("12/345")]
    [InlineData("12/345/6789/extra")]
    [InlineData("12//6789")]
    public async Task Translate_returns_null_for_malformed_site_identifier(string siteIdentifier)
    {
        var result = await _translator.Translate(Request(siteIdentifier: siteIdentifier), CancellationToken.None);

        Assert.Null(result);
        Assert.Empty(_client.ReceivedCalls());
    }

    [Fact]
    public async Task Translate_returns_null_when_holding_not_found()
    {
        _client.GetCattleForHoldingMultiSegmentAsync(County, Parish, Holding, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Translate_propagates_service_errors(HttpStatusCode statusCode)
    {
        _client.GetCattleForHoldingMultiSegmentAsync(County, Parish, Holding, null, null, null, Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(statusCode));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => _translator.Translate(Request(), CancellationToken.None));

        Assert.Equal(statusCode, ex.StatusCode);
    }

    [Fact]
    public async Task Translate_returns_empty_result_when_holding_has_no_cattle()
    {
        SetupCattle();

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(0, result.Count);
        Assert.Empty(result.Values!);
    }

    [Fact]
    public async Task Translate_maps_cattle_to_animal_summary()
    {
        SetupCattle(Cattle("UK12345678901", sex: "Male", status: "dead", breedCode: "AA", breedName: "Aberdeen Angus"));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        var summary = Assert.Single(result!.Values!);
        Assert.Equal("UK12345678901", summary.Animal!.Identifier);
        Assert.Equal(Species.Cattle, summary.Animal.Species);
        Assert.Equal(Sex.Male, summary.Animal.Sex);
        Assert.Equal(AnimalState.Dead, summary.Animal.State);
        Assert.NotNull(summary.Breed);
        Assert.Equal("AA", summary.Breed.Code);
        Assert.Equal("Aberdeen Angus", summary.Breed.Name);
    }

    [Theory]
    [InlineData("registered", AnimalState.Registered)]
    [InlineData("DEAD", AnimalState.Dead)]
    [InlineData("lost", AnimalState.Lost)]
    [InlineData("Stolen", AnimalState.Stolen)]
    [InlineData("unknown", null)]
    [InlineData("", null)]
    public async Task Translate_maps_status_to_animal_state(string status, AnimalState? expectedState)
    {
        SetupCattle(Cattle("UK1", status: status));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.Equal(expectedState, Assert.Single(result!.Values!).Animal!.State);
    }

    [Theory]
    [InlineData("female", Sex.Female)]
    [InlineData("MALE", Sex.Male)]
    public async Task Translate_maps_sex_ignoring_case(string sex, Sex expectedSex)
    {
        SetupCattle(Cattle("UK1", sex: sex));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.Equal(expectedSex, Assert.Single(result!.Values!).Animal!.Sex);
    }

    [Fact]
    public async Task Translate_skips_cattle_with_missing_or_unsupported_sex()
    {
        SetupCattle(
            Cattle("UK1", sex: "female"),
            Cattle("UK2", sex: null),
            Cattle("UK3", sex: "unknown"),
            Cattle("UK4", sex: "  "),
            Cattle("UK5", sex: "male"));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.Equal(2, result!.Count);
        Assert.Equal(["UK1", "UK5"], Ids(result));
    }

    [Fact]
    public async Task Translate_returns_null_breed_when_no_breed_code_or_name()
    {
        SetupCattle(Cattle("UK1"));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.Null(Assert.Single(result!.Values!).Breed);
    }

    [Theory]
    [InlineData("AA", null)]
    [InlineData(null, "Aberdeen Angus")]
    public async Task Translate_returns_breed_when_either_code_or_name_present(string? code, string? name)
    {
        SetupCattle(Cattle("UK1", breedCode: code, breedName: name));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        var breed = Assert.Single(result!.Values!).Breed;
        Assert.NotNull(breed);
        Assert.Equal(code, breed.Code);
        Assert.Equal(name, breed.Name);
    }

    [Fact]
    public async Task Translate_orders_by_identifier_ascending_by_default()
    {
        SetupCattle(Cattle("UK3"), Cattle("UK1"), Cattle("UK2"));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.Equal(["UK1", "UK2", "UK3"], Ids(result!));
    }

    [Theory]
    [InlineData("desc")]
    [InlineData("DESC")]
    public async Task Translate_orders_by_identifier_descending_when_requested(string sort)
    {
        SetupCattle(Cattle("UK1"), Cattle("UK3"), Cattle("UK2"));

        var result = await _translator.Translate(Request(sort: sort), CancellationToken.None);

        Assert.Equal(["UK3", "UK2", "UK1"], Ids(result!));
    }

    [Fact]
    public async Task Translate_orders_by_identifier_ignoring_case()
    {
        SetupCattle(Cattle("uk2"), Cattle("UK1"), Cattle("Uk3"));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.Equal(["UK1", "uk2", "Uk3"], Ids(result!));
    }

    [Fact]
    public async Task Translate_orders_by_state_when_requested()
    {
        SetupCattle(
            Cattle("UK1", status: "registered"),
            Cattle("UK2", status: "lost"),
            Cattle("UK3", status: "dead"));

        var result = await _translator.Translate(Request(order: AnimalOrder.State), CancellationToken.None);

        Assert.Equal(["UK3", "UK2", "UK1"], Ids(result!));
    }

    [Fact]
    public async Task Translate_orders_by_state_descending_when_requested()
    {
        SetupCattle(
            Cattle("UK1", status: "registered"),
            Cattle("UK2", status: "lost"),
            Cattle("UK3", status: "dead"));

        var result = await _translator.Translate(
            Request(order: AnimalOrder.State, sort: "desc"),
            CancellationToken.None);

        Assert.Equal(["UK1", "UK2", "UK3"], Ids(result!));
    }

    [Fact]
    public async Task Translate_returns_all_animals_when_page_size_not_specified()
    {
        SetupCattle(Cattle("UK1"), Cattle("UK2"), Cattle("UK3"));

        var result = await _translator.Translate(Request(), CancellationToken.None);

        Assert.Equal(3, result!.Count);
        Assert.Equal(3, result.Values!.Count);
    }

    [Fact]
    public async Task Translate_returns_first_page_of_results()
    {
        SetupCattle(Cattle("UK1"), Cattle("UK2"), Cattle("UK3"), Cattle("UK4"), Cattle("UK5"));

        var result = await _translator.Translate(Request(page: 1, pageSize: 2), CancellationToken.None);

        Assert.Equal(5, result!.Count);
        Assert.Equal(["UK1", "UK2"], Ids(result));
    }

    [Fact]
    public async Task Translate_returns_subsequent_page_of_results()
    {
        SetupCattle(Cattle("UK1"), Cattle("UK2"), Cattle("UK3"), Cattle("UK4"), Cattle("UK5"));

        var result = await _translator.Translate(Request(page: 2, pageSize: 2), CancellationToken.None);

        Assert.Equal(5, result!.Count);
        Assert.Equal(["UK3", "UK4"], Ids(result));
    }

    [Fact]
    public async Task Translate_returns_partial_last_page()
    {
        SetupCattle(Cattle("UK1"), Cattle("UK2"), Cattle("UK3"), Cattle("UK4"), Cattle("UK5"));

        var result = await _translator.Translate(Request(page: 3, pageSize: 2), CancellationToken.None);

        Assert.Equal(5, result!.Count);
        Assert.Equal(["UK5"], Ids(result));
    }

    [Fact]
    public async Task Translate_returns_no_values_for_page_beyond_results_but_keeps_total_count()
    {
        SetupCattle(Cattle("UK1"), Cattle("UK2"), Cattle("UK3"), Cattle("UK4"), Cattle("UK5"));

        var result = await _translator.Translate(Request(page: 4, pageSize: 2), CancellationToken.None);

        Assert.Equal(5, result!.Count);
        Assert.Empty(result.Values!);
    }

    [Fact]
    public async Task Translate_returns_all_animals_when_page_size_exceeds_total()
    {
        SetupCattle(Cattle("UK1"), Cattle("UK2"));

        var result = await _translator.Translate(Request(pageSize: 10), CancellationToken.None);

        Assert.Equal(2, result!.Count);
        Assert.Equal(["UK1", "UK2"], Ids(result));
    }

    [Fact]
    public async Task Translate_paginates_after_sorting()
    {
        SetupCattle(Cattle("UK1"), Cattle("UK2"), Cattle("UK3"));

        var result = await _translator.Translate(Request(page: 1, pageSize: 1, sort: "desc"), CancellationToken.None);

        Assert.Equal(3, result!.Count);
        Assert.Equal(["UK3"], Ids(result));
    }
}
