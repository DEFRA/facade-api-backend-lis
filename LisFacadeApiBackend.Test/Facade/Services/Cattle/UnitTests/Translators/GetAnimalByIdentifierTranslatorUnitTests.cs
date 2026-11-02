using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.Translators;

public class GetAnimalByIdentifierTranslatorUnitTests
{
    private const string Identifier = "UK12345678901";
    private const string DefaultSpecies = "cattle";
    private const string DefaultSex = "female";
    private static readonly DateOnly RegistrationDate = new(2024, 3, 15);

    private readonly ICattleApiClient _client = Substitute.For<ICattleApiClient>();
    private readonly GetAnimalByIdentifierTranslator _translator;

    public GetAnimalByIdentifierTranslatorUnitTests()
    {
        var service = new GetAnimalByIdentifierService(_client, NullLogger<GetAnimalByIdentifierService>.Instance);
        _translator = new GetAnimalByIdentifierTranslator(service, NullLogger<GetAnimalByIdentifierTranslator>.Instance);
    }

    private static CattleDetailsResponse FullResponse() => new()
    {
        EarTag = Identifier,
        Species = DefaultSpecies,
        Sex = DefaultSex,
        State = "registered",
        BreedCode = "AA",
        BreedName = "Aberdeen Angus",
        DateRegistered = RegistrationDate,
        CurrentLocation = new CattleLocationResponse { Identifier = "11/111/1111", Name = "Current Farm", PremisesType = "AH" },
        RegisteredLocation = new CattleLocationResponse { Identifier = "22/222/2222", Name = "Reg Farm", PremisesType = "AB" },
        DeathLocation = new CattleLocationResponse { Identifier = "33/333/3333", Name = "Abattoir", PremisesType = "SL" },
        SireEarTag = "UK-SIRE",
        SireName = "Big Bull",
        GeneticDamEarTag = "UK-GDAM",
        SurrogateDamEarTag = "UK-SDAM"
    };

    private void SetupResponse(HttpResponseMessage response) =>
        _client.GetCattleDetailsAsync(Identifier, Arg.Any<CancellationToken>()).Returns(response);

    [Fact]
    public void TranslateRequest_returns_identifier_unchanged()
    {
        Assert.Equal(Identifier, _translator.TranslateRequest(Identifier));
    }

    [Fact]
    public async Task Translate_returns_null_when_cattle_not_found()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await _translator.Translate(Identifier, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Translate_returns_translated_animal_when_cattle_found()
    {
        SetupResponse(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(FullResponse()) });

        var result = await _translator.Translate(Identifier, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(Identifier, result.AnimalDetails.Identifier);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task Translate_propagates_service_errors(HttpStatusCode statusCode)
    {
        SetupResponse(new HttpResponseMessage(statusCode));

        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => _translator.Translate(Identifier, CancellationToken.None));

        Assert.Equal(statusCode, ex.StatusCode);
    }

    [Fact]
    public void TranslateResponse_maps_animal_details()
    {
        var result = _translator.TranslateResponse(FullResponse());

        Assert.Equal(Identifier, result.AnimalDetails.Identifier);
        Assert.Equal(Species.Cattle, result.AnimalDetails.Species);
        Assert.Equal(Sex.Female, result.AnimalDetails.Sex);
        Assert.Equal(AnimalState.Registered, result.AnimalDetails.State);
    }

    [Theory]
    [InlineData("bison", Species.Bison)]
    [InlineData("buffalo", Species.Buffalo)]
    [InlineData("cattle", Species.Cattle)]
    [InlineData("deer", Species.Deer)]
    [InlineData("goats", Species.Goats)]
    [InlineData("sheep", Species.Sheep)]
    [InlineData("CATTLE", Species.Cattle)]
    [InlineData(" cattle ", Species.Cattle)]
    public void TranslateResponse_parses_supported_species(string value, Species expected)
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = value, Sex = DefaultSex, DateRegistered = RegistrationDate });

        Assert.Equal(expected, result.AnimalDetails.Species);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("unknown")]
    [InlineData("0")]
    [InlineData("999")]
    public void TranslateResponse_rejects_missing_or_unsupported_species(string? value)
    {
        var cattle = new CattleDetailsResponse { EarTag = Identifier, Species = value, Sex = DefaultSex, DateRegistered = RegistrationDate };

        var ex = Assert.Throws<InvalidOperationException>(() => _translator.TranslateResponse(cattle));

        Assert.Equal("Upstream cattle response has missing or unsupported species.", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("unknown")]
    [InlineData("0")]
    [InlineData("999")]
    public void TranslateResponse_rejects_missing_or_unsupported_sex(string? value)
    {
        var cattle = new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = value, DateRegistered = RegistrationDate };

        var ex = Assert.Throws<InvalidOperationException>(() => _translator.TranslateResponse(cattle));

        Assert.Equal("Upstream cattle response has missing or unsupported sex.", ex.Message);
    }

    [Theory]
    [InlineData("MALE", Sex.Male)]
    [InlineData(" female ", Sex.Female)]
    public void TranslateResponse_parses_sex_case_insensitively(string value, Sex expected)
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = value, DateRegistered = RegistrationDate });

        Assert.Equal(expected, result.AnimalDetails.Sex);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("unknown")]
    public void TranslateResponse_keeps_missing_or_unsupported_optional_state_null(string? value)
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = DefaultSex, DateRegistered = RegistrationDate, State = value });

        Assert.Null(result.AnimalDetails.State);
    }

    [Fact]
    public void TranslateResponse_maps_breed()
    {
        var result = _translator.TranslateResponse(FullResponse());

        Assert.NotNull(result.Breed);
        Assert.Equal("AA", result.Breed.Code);
        Assert.Equal("Aberdeen Angus", result.Breed.Name);
    }

    [Theory]
    [InlineData("AA", null)]
    [InlineData(null, "Aberdeen Angus")]
    public void TranslateResponse_maps_breed_when_only_one_part_present(string? code, string? name)
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = DefaultSex, DateRegistered = RegistrationDate, BreedCode = code, BreedName = name });

        Assert.NotNull(result.Breed);
        Assert.Equal(code, result.Breed.Code);
        Assert.Equal(name, result.Breed.Name);
    }

    [Fact]
    public void TranslateResponse_breed_is_null_when_code_and_name_missing()
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = DefaultSex, DateRegistered = RegistrationDate, Breed = "Angus" });

        Assert.Null(result.Breed);
    }

    [Fact]
    public void TranslateResponse_maps_current_site()
    {
        var result = _translator.TranslateResponse(FullResponse());

        Assert.NotNull(result.CurrentSite);
        Assert.Equal("Current Farm", result.CurrentSite.Name);
        Assert.Equal("AH", result.CurrentSite.Type.Type);
        Assert.Equal(string.Empty, result.CurrentSite.Type.Description);
        var siteIdentifier = Assert.Single(result.CurrentSite.Identifiers);
        Assert.Equal("11/111/1111", siteIdentifier.Identifier);
        Assert.Equal(string.Empty, siteIdentifier.Type);
    }

    [Fact]
    public void TranslateResponse_current_site_is_null_when_no_current_location()
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = DefaultSex, DateRegistered = RegistrationDate });

        Assert.Null(result.CurrentSite);
    }

    [Fact]
    public void TranslateResponse_site_has_no_identifiers_when_location_identifier_missing()
    {
        var result = _translator.TranslateResponse(new CattleDetailsResponse
        {
            EarTag = Identifier,
            Species = DefaultSpecies,
            Sex = DefaultSex,
            DateRegistered = RegistrationDate,
            CurrentLocation = new CattleLocationResponse { Name = "No Id", PremisesType = null }
        });

        Assert.NotNull(result.CurrentSite);
        Assert.Empty(result.CurrentSite.Identifiers);
        Assert.Equal(string.Empty, result.CurrentSite.Type.Type);
    }

    [Fact]
    public void TranslateResponse_maps_death_site()
    {
        var result = _translator.TranslateResponse(FullResponse());

        Assert.NotNull(result.Death);
        var site = result.Death.Site;
        Assert.NotNull(site);
        Assert.Equal("Abattoir", site.Name);
        Assert.Equal("SL", site.Type.Type);
        Assert.Equal("33/333/3333", Assert.Single(site.Identifiers).Identifier);
    }

    [Fact]
    public void TranslateResponse_death_is_null_when_no_death_location()
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = DefaultSex, DateRegistered = RegistrationDate });

        Assert.Null(result.Death);
    }

    [Fact]
    public void TranslateResponse_maps_import_parents()
    {
        var result = _translator.TranslateResponse(FullResponse());

        Assert.NotNull(result.ImportParents);
        Assert.Equal("UK-SIRE", result.ImportParents.Sire?.Identifier);
        Assert.Equal("Big Bull", result.ImportParents.Sire?.Name);
        Assert.Equal("UK-GDAM", result.ImportParents.GeneticDam?.Identifier);
        Assert.Null(result.ImportParents.GeneticDam?.Name);
        Assert.Equal("UK-SDAM", result.ImportParents.BirthDam?.Identifier);
    }

    [Fact]
    public void TranslateResponse_import_parents_is_null_when_no_parents()
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = DefaultSex, DateRegistered = RegistrationDate });

        Assert.Null(result.ImportParents);
    }

    [Fact]
    public void TranslateResponse_import_parents_present_when_only_one_parent_known()
    {
        var result = _translator.TranslateResponse(
            new CattleDetailsResponse { EarTag = Identifier, Species = DefaultSpecies, Sex = DefaultSex, DateRegistered = RegistrationDate, GeneticDamEarTag = "UK-GDAM" });

        Assert.NotNull(result.ImportParents);
        Assert.Null(result.ImportParents.Sire);
        Assert.Equal("UK-GDAM", result.ImportParents.GeneticDam?.Identifier);
        Assert.Null(result.ImportParents.BirthDam);
    }

    [Fact]
    public void TranslateResponse_rejects_missing_registration_date()
    {
        var cattle = new CattleDetailsResponse
        {
            EarTag = Identifier,
            Species = DefaultSpecies,
            Sex = DefaultSex,
            DateRegistered = null
        };

        var ex = Assert.Throws<InvalidOperationException>(() => _translator.TranslateResponse(cattle));

        Assert.Equal("Upstream cattle response has missing registration date.", ex.Message);
    }

    [Fact]
    public void TranslateResponse_maps_registration()
    {
        var result = _translator.TranslateResponse(FullResponse());

        Assert.Equal(RegistrationDate, result.Registration.Date);
        Assert.Equal("UK-SDAM", result.Registration.BirthDam?.Identifier);
        Assert.Equal("UK-GDAM", result.Registration.GeneticDam?.Identifier);
        Assert.Equal("UK-SIRE", result.Registration.Sire?.Identifier);
        Assert.Equal("Big Bull", result.Registration.Sire?.Name);
        Assert.Equal("Reg Farm", result.Registration.Site.Name);
        Assert.Equal("22/222/2222", Assert.Single(result.Registration.Site.Identifiers).Identifier);
    }
}
