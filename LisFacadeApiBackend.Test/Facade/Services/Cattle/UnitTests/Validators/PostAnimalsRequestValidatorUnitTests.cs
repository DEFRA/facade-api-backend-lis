using System.Text.Json;
using LisFacadeApiBackend.Facade.Endpoints.Validators;
using LisFacadeApiBackend.Facade.LIS.Animal;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Request;
using AnimalModel = LisFacadeApiBackend.Facade.LIS.Request.CreateAnimalRequest;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.Validators;

public class PostAnimalsRequestValidatorUnitTests
{
    private const string Cph = "11/222/3333";

    private static AnimalModel BuildAnimal(
        string cph = Cph,
        DateOnly? birthDate = null,
        DateOnly? registrationDate = null,
        RegistrationCategory category = RegistrationCategory.Registration,
        Sex sex = Sex.Female) => new()
    {
        AnimalDetails = new AnimalRecordDetails { Identifier = "UK12345678901", Species = Species.Cattle, Sex = sex },
        Birth = birthDate is null
            ? null
            : new CreateAnimalBirthRequest
            {
                Site = new CreateSiteRequest { Identifiers = [] },
                Date = birthDate.Value,
                AssistedBirthFlag = false,
                MultipleBirthsFlag = false,
                EmbryoTransferFlag = false
            },
        Registration = new CreateAnimalRegistrationRequest
        {
            Date = registrationDate ?? new DateOnly(2024, 3, 15),
            Category = category,
            Site = new CreateSiteRequest
            {
                Identifiers = [new CreateSiteIdentifierRequest { Identifier = cph }]
            }
        }
    };

    [Fact]
    public void Builds_request_when_all_checks_pass()
    {
        Assert.True(PostAnimalsRequestValidator.TryBuild(BuildAnimal(), out _, out _));
    }

    [Fact]
    public void Accepts_body_without_read_only_site_type_metadata()
    {
        const string json = """
            {
              "animal": { "identifier": "UK12345678901", "species": "cattle", "sex": "female" },
              "registration": {
                "date": "2024-03-15",
                "category": "registration",
                "site": { "identifiers": [ { "identifier": "11/222/3333" } ] }
              }
            }
            """;

        var animal = JsonSerializer.Deserialize<AnimalModel>(json);

        Assert.NotNull(animal);
        Assert.True(PostAnimalsRequestValidator.TryBuild(animal, out _, out _));
    }

    [Theory]
    [InlineData("11/222")]
    [InlineData("11/222/3333/4")]
    [InlineData("11//3333")]
    [InlineData("11/./3333")]
    [InlineData("11-222-3333")]
    public void Rejects_cph_not_in_county_parish_holding_format(string cph)
    {
        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimal(cph: cph), out var request, out var error);

        Assert.False(built);
        Assert.Null(request);
        Assert.Equal("registration.site identifier must be in county/parish/holding format", error);
    }

    [Fact]
    public void Accepts_cph_when_non_cph_identifier_comes_first()
    {
        var site = new CreateSiteRequest
        {
            Identifiers = [new CreateSiteIdentifierRequest { Identifier = "FSA12345" }, new CreateSiteIdentifierRequest { Identifier = Cph }]
        };

        Assert.True(PostAnimalsRequestValidator.TryBuild(BuildAnimalWithSite(site), out _, out _));
    }

    [Fact]
    public void Rejects_multiple_cph_identifiers_as_ambiguous()
    {
        var site = new CreateSiteRequest
        {
            Identifiers = [new CreateSiteIdentifierRequest { Identifier = Cph }, new CreateSiteIdentifierRequest { Identifier = "22/333/4444" }]
        };

        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimalWithSite(site), out var request, out var error);

        Assert.False(built);
        Assert.Null(request);
        Assert.Equal("registration.site must contain only one county/parish/holding identifier", error);
    }

    [Fact]
    public void Accepts_birth_date_one_day_ahead_of_utc_for_timezones_ahead_of_utc()
    {
        var tomorrowUtc = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        Assert.True(PostAnimalsRequestValidator.TryBuild(BuildAnimal(birthDate: tomorrowUtc), out _, out _));
    }

    [Fact]
    public void Rejects_birth_date_more_than_one_day_ahead_of_utc()
    {
        var twoDaysAhead = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2);

        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimal(birthDate: twoDaysAhead), out _, out var error);

        Assert.False(built);
        Assert.Equal("birth.date cannot be in the future", error);
    }

    [Fact]
    public void Rejects_registration_date_left_at_default()
    {
        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimal(registrationDate: DateOnly.MinValue), out _, out var error);

        Assert.False(built);
        Assert.Equal("registration.date is required", error);
    }

    [Fact]
    public void Rejects_undefined_category()
    {
        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimal(category: (RegistrationCategory)99), out _, out var error);

        Assert.False(built);
        Assert.Equal("registration.category must be a supported category", error);
    }

    [Fact]
    public void Rejects_undefined_sex()
    {
        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimal(sex: (Sex)99), out _, out var error);

        Assert.False(built);
        Assert.Equal("animal.sex must be a supported sex", error);
    }

    [Fact]
    public void Rejects_null_animal_details()
    {
        var animal = new AnimalModel { AnimalDetails = null!, Registration = BuildRegistration(BuildSite()) };

        var built = PostAnimalsRequestValidator.TryBuild(animal, out _, out var error);

        Assert.False(built);
        Assert.Equal("animal is required", error);
    }

    [Fact]
    public void Rejects_null_registration()
    {
        var animal = new AnimalModel
        {
            AnimalDetails = new AnimalRecordDetails { Identifier = "UK12345678901", Species = Species.Cattle, Sex = Sex.Female },
            Registration = null!
        };

        var built = PostAnimalsRequestValidator.TryBuild(animal, out _, out var error);

        Assert.False(built);
        Assert.Equal("registration is required", error);
    }

    [Fact]
    public void Rejects_null_registration_site()
    {
        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimalWithSite(null!), out _, out var error);

        Assert.False(built);
        Assert.Equal("registration.site is required", error);
    }

    [Fact]
    public void Rejects_null_site_identifiers()
    {
        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimalWithSite(new CreateSiteRequest { Identifiers = null! }), out _, out var error);

        Assert.False(built);
        Assert.Equal("registration.site.identifiers is required", error);
    }

    [Fact]
    public void Rejects_null_site_identifier_entry()
    {
        var site = new CreateSiteRequest { Identifiers = [null!, new CreateSiteIdentifierRequest { Identifier = Cph }] };

        var built = PostAnimalsRequestValidator.TryBuild(BuildAnimalWithSite(site), out _, out var error);

        Assert.False(built);
        Assert.Equal("registration.site.identifiers must not contain null entries", error);
    }

    [Fact]
    public void Rejects_null_birth_site()
    {
        var animal = new AnimalModel
        {
            AnimalDetails = new AnimalRecordDetails { Identifier = "UK12345678901", Species = Species.Cattle, Sex = Sex.Female },
            Birth = new CreateAnimalBirthRequest
            {
                Site = null!,
                AssistedBirthFlag = false,
                MultipleBirthsFlag = false,
                EmbryoTransferFlag = false
            },
            Registration = BuildRegistration(BuildSite())
        };

        var built = PostAnimalsRequestValidator.TryBuild(animal, out _, out var error);

        Assert.False(built);
        Assert.Equal("birth.site is required", error);
    }

    [Fact]
    public void Rejects_null_registration_parent_identifier()
    {
        var animal = new AnimalModel
        {
            AnimalDetails = new AnimalRecordDetails { Identifier = "UK12345678901", Species = Species.Cattle, Sex = Sex.Female },
            Registration = new CreateAnimalRegistrationRequest
            {
                Date = new DateOnly(2024, 3, 15),
                Category = RegistrationCategory.Registration,
                Site = BuildSite(),
                Sire = new ParentAnimal { Identifier = null! }
            }
        };

        var built = PostAnimalsRequestValidator.TryBuild(animal, out _, out var error);

        Assert.False(built);
        Assert.Equal("registration.sire.identifier is required", error);
    }

    [Fact]
    public void Rejects_null_import_parent_identifier()
    {
        var animal = new AnimalModel
        {
            AnimalDetails = new AnimalRecordDetails { Identifier = "UK12345678901", Species = Species.Cattle, Sex = Sex.Female },
            ImportParents = new ImportParents { BirthDam = new ParentAnimal { Identifier = null! } },
            Registration = BuildRegistration(BuildSite())
        };

        var built = PostAnimalsRequestValidator.TryBuild(animal, out _, out var error);

        Assert.False(built);
        Assert.Equal("importParents.birthDam.identifier is required", error);
    }

    [Fact]
    public void Rejects_null_birth_site_identifier_entry_identifier()
    {
        var animal = new AnimalModel
        {
            AnimalDetails = new AnimalRecordDetails { Identifier = "UK12345678901", Species = Species.Cattle, Sex = Sex.Female },
            Birth = new CreateAnimalBirthRequest
            {
                Site = new CreateSiteRequest { Identifiers = [new CreateSiteIdentifierRequest { Identifier = null! }] },
                AssistedBirthFlag = false,
                MultipleBirthsFlag = false,
                EmbryoTransferFlag = false
            },
            Registration = BuildRegistration(BuildSite())
        };

        var built = PostAnimalsRequestValidator.TryBuild(animal, out _, out var error);

        Assert.False(built);
        Assert.Equal("birth.site.identifiers must not contain null identifiers", error);
    }

    private static CreateSiteRequest BuildSite() =>
        new() { Identifiers = [new CreateSiteIdentifierRequest { Identifier = Cph }] };

    private static CreateAnimalRegistrationRequest BuildRegistration(CreateSiteRequest site) => new()
    {
        Date = new DateOnly(2024, 3, 15),
        Category = RegistrationCategory.Registration,
        Site = site
    };

    private static AnimalModel BuildAnimalWithSite(CreateSiteRequest site) => new()
    {
        AnimalDetails = new AnimalRecordDetails { Identifier = "UK12345678901", Species = Species.Cattle, Sex = Sex.Female },
        Registration = BuildRegistration(site)
    };
}
