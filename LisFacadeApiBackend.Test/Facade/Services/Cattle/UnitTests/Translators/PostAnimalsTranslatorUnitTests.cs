using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Request;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;
using LisFacadeApiBackend.Facade.Services.Cattle.EndpointServices;
using LisFacadeApiBackend.Facade.Services.Cattle.Translators.Animal;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using AnimalModel = LisFacadeApiBackend.Facade.LIS.Request.CreateAnimalRequest;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.Translators;

public class PostAnimalsTranslatorUnitTests
{
    private const string EarTag = "UK12345678901";
    private const string Cph = "11/222/3333";
    private static readonly Guid BundleId = Guid.NewGuid();

    private readonly ICattleApiClient _client = Substitute.For<ICattleApiClient>();
    private readonly PostAnimalsTranslator _translator;

    public PostAnimalsTranslatorUnitTests()
    {
        var service = new PostAnimalsService(_client, NullLogger<PostAnimalsService>.Instance);
        _translator = new PostAnimalsTranslator(service, NullLogger<PostAnimalsTranslator>.Instance);
    }

    private static AnimalModel Animal(
        string earTag = EarTag,
        Sex sex = Sex.Female,
        Breed? breed = null,
        DateOnly? birthDate = null,
        ParentAnimal? birthDam = null,
        ParentAnimal? geneticDam = null,
        ParentAnimal? sire = null,
        ImportParents? importParents = null,
        List<CreateSiteIdentifierRequest>? siteIdentifiers = null) => new()
    {
        AnimalDetails = new AnimalRecordDetails { Identifier = earTag, Species = Species.Cattle, Sex = sex },
        Breed = breed ?? new Breed { Code = "AA", Name = "Aberdeen Angus" },
        Birth = birthDate is null
            ? null
            : new CreateAnimalBirthRequest
            {
                Site = new CreateSiteRequest { Identifiers = [] },
                Date = birthDate,
                AssistedBirthFlag = false,
                MultipleBirthsFlag = false,
                EmbryoTransferFlag = false
            },
        ImportParents = importParents,
        Registration = new CreateAnimalRegistrationRequest
        {
            Date = new DateOnly(2024, 3, 15),
            Category = RegistrationCategory.Registration,
            Site = new CreateSiteRequest
            {
                Identifiers = siteIdentifiers ?? new List<CreateSiteIdentifierRequest> { new() { Identifier = Cph } }
            },
            BirthDam = birthDam,
            GeneticDam = geneticDam,
            Sire = sire
        }
    };

    private void SetupCreate(HttpStatusCode status, Guid id) =>
        _client.CreateRegistrationBundleAsync(Arg.Any<RegistrationBundleRequest>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(status)
            {
                Content = JsonContent.Create(new BundleResponse { Id = id })
            });

    [Fact]
    public async Task Translate_returns_animal_response_after_creating_bundle()
    {
        SetupCreate(HttpStatusCode.Created, BundleId);

        var result = await _translator.Translate(Animal(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(EarTag, result.Animal.Identifier);
        Assert.Equal($"/animals/{EarTag}", result.Animal.Links?.Self);
    }

    [Fact]
    public async Task Translate_propagates_failure_when_bundle_creation_fails()
    {
        SetupCreate(HttpStatusCode.BadRequest, BundleId);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => _translator.Translate(Animal(), CancellationToken.None));
    }

    [Fact]
    public async Task TranslateRequest_maps_cph_breed_and_falls_back_to_import_parents()
    {
        var importParents = new ImportParents
        {
            Sire = new ParentAnimal { Identifier = "UK-SIRE" },
            BirthDam = new ParentAnimal { Identifier = "UK-IMPORT-DAM" }
        };

        var request = _translator.TranslateRequest(Animal(importParents: importParents));

        Assert.Equal(Cph, request.Holding?.Cph);
        var animal = Assert.Single(request.Animals);
        Assert.Equal(EarTag, animal.EarTag);
        Assert.Equal("female", animal.Sex);
        Assert.Equal("AA", animal.Breed);
        Assert.Equal("UK-IMPORT-DAM", animal.Dam?.SurrogateDamEarTag);
        Assert.Equal("UK-SIRE", animal.Sire?.EarTag);
    }

    [Fact]
    public async Task Translate_sends_registration_bundle_for_the_animal()
    {
        SetupCreate(HttpStatusCode.Created, BundleId);

        await _translator.Translate(Animal(), CancellationToken.None);

        await _client.Received(1).CreateRegistrationBundleAsync(
            Arg.Is<RegistrationBundleRequest>(r => r.Holding!.Cph == Cph && r.Animals.Single().EarTag == EarTag),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Translate_escapes_ear_tag_in_self_link()
    {
        SetupCreate(HttpStatusCode.Created, BundleId);

        var result = await _translator.Translate(Animal(earTag: "UK 12/34"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("UK 12/34", result.Animal.Identifier);
        Assert.Equal("/animals/UK%2012%2F34", result.Animal.Links?.Self);
    }

    [Fact]
    public void TranslateRequest_selects_cph_regardless_of_identifier_order()
    {
        var fsa = new CreateSiteIdentifierRequest { Identifier = "FSA12345" };
        var cphEntry = new CreateSiteIdentifierRequest { Identifier = Cph };

        Assert.Equal(Cph, _translator.TranslateRequest(Animal(siteIdentifiers: [fsa, cphEntry])).Holding?.Cph);
        Assert.Equal(Cph, _translator.TranslateRequest(Animal(siteIdentifiers: [cphEntry, fsa])).Holding?.Cph);
    }

    [Theory]
    [InlineData(Sex.Female, "female")]
    [InlineData(Sex.Male, "male")]
    public void TranslateRequest_maps_sex_to_lowercase_string(Sex sex, string expected)
    {
        var request = _translator.TranslateRequest(Animal(sex: sex));

        Assert.Equal(expected, Assert.Single(request.Animals).Sex);
    }

    [Fact]
    public void TranslateRequest_maps_date_of_birth_from_birth_record()
    {
        var request = _translator.TranslateRequest(Animal(birthDate: new DateOnly(2023, 1, 2)));

        Assert.Equal(new DateOnly(2023, 1, 2), Assert.Single(request.Animals).DateOfBirth);
    }

    [Fact]
    public void TranslateRequest_omits_breed_when_code_is_missing()
    {
        var request = _translator.TranslateRequest(Animal(breed: new Breed { Code = null, Name = "Aberdeen Angus" }));

        Assert.Null(Assert.Single(request.Animals).Breed);
    }

    [Fact]
    public void TranslateRequest_uses_registration_genetic_dam_over_import_parents()
    {
        var animal = Animal(
            geneticDam: new ParentAnimal { Identifier = "REG-DAM" },
            importParents: new ImportParents { GeneticDam = new ParentAnimal { Identifier = "IMP-DAM" } });

        var request = _translator.TranslateRequest(animal);

        Assert.Equal("REG-DAM", Assert.Single(request.Animals).Dam?.GeneticDamEarTag);
    }

    [Fact]
    public void TranslateRequest_prefers_import_parents_surrogate_dam_over_registration()
    {
        var animal = Animal(
            birthDam: new ParentAnimal { Identifier = "REG-BIRTH-DAM" },
            importParents: new ImportParents { BirthDam = new ParentAnimal { Identifier = "IMP-BIRTH-DAM" } });

        var request = _translator.TranslateRequest(animal);

        Assert.Equal("IMP-BIRTH-DAM", Assert.Single(request.Animals).Dam?.SurrogateDamEarTag);
    }

    [Fact]
    public void TranslateRequest_falls_back_to_registration_birth_dam_as_surrogate_dam()
    {
        var animal = Animal(birthDam: new ParentAnimal { Identifier = "REG-BIRTH-DAM" });

        var request = _translator.TranslateRequest(animal);

        Assert.Equal("REG-BIRTH-DAM", Assert.Single(request.Animals).Dam?.SurrogateDamEarTag);
    }

    [Fact]
    public void TranslateRequest_falls_back_to_import_parents_genetic_dam()
    {
        var animal = Animal(
            importParents: new ImportParents { GeneticDam = new ParentAnimal { Identifier = "IMP-DAM" } });

        var request = _translator.TranslateRequest(animal);

        Assert.Equal("IMP-DAM", Assert.Single(request.Animals).Dam?.GeneticDamEarTag);
    }

    [Fact]
    public void TranslateRequest_omits_dam_when_no_dam_is_recorded()
    {
        var request = _translator.TranslateRequest(Animal());

        Assert.Null(Assert.Single(request.Animals).Dam);
    }

    [Fact]
    public void TranslateRequest_prefers_registration_sire_over_import_parents()
    {
        var animal = Animal(
            sire: new ParentAnimal { Identifier = "REG-SIRE" },
            importParents: new ImportParents { Sire = new ParentAnimal { Identifier = "IMP-SIRE" } });

        var request = _translator.TranslateRequest(animal);

        Assert.Equal("REG-SIRE", Assert.Single(request.Animals).Sire?.EarTag);
    }

    [Fact]
    public void TranslateRequest_omits_sire_when_none_is_recorded()
    {
        var request = _translator.TranslateRequest(Animal());

        Assert.Null(Assert.Single(request.Animals).Sire);
    }
}
