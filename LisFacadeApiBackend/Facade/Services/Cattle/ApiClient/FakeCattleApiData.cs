using System.Net;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.Cattle.Response.Cattle;
using LisFacadeApiBackend.Facade.Cattle.Response.HealthCheck;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.Cattle.Response.Users;

namespace LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

public static class FakeCattleApiData
{
    public static IReadOnlyDictionary<string, CattleDetailsResponse> GetCattleByEarTag() =>
        new Dictionary<string, CattleDetailsResponse>(StringComparer.OrdinalIgnoreCase)
        {
            ["UK12345678901"] = new()
            {
                EarTag = "UK12345678901",
                Species = "Cattle",
                Sex = "Female",
                DateBirth = new DateOnly(2024, 1, 15),
                DateRegistered = new DateOnly(2024, 2, 1),
                DateOnCph = new DateOnly(2024, 2, 1),
                Breed = "Holstein",
                BreedCode = "HOL",
                BreedName = "Holstein Friesian",
                State = "Alive",
                RestrictionStatus = "None",
                CurrentLocation = new() { Identifier = "12/345/6789", SubIdentifier = "01", FullIdentifier = "12/345/6789/01", Name = "Fake Farm", PremisesType = "Farm" },
                RegisteredLocation = new() { Identifier = "12/345/6789", SubIdentifier = "01", FullIdentifier = "12/345/6789/01", Name = "Fake Farm", PremisesType = "Farm" },
                DeathLocation = new() { Identifier = "98/765/4321", SubIdentifier = "01", FullIdentifier = "98/765/4321/01", Name = "Fake Abattoir", PremisesType = "Abattoir" },
                DamType = "Genetic",
                GeneticDamEarTag = "UK12345678800",
                SurrogateDamEarTag = "UK12345678801",
                SireEarTag = "UK12345678700",
                SireName = "Fake Sire One"
            },
            ["UK12345678902"] = new()
            {
                EarTag = "UK12345678902",
                Species = "Cattle",
                Sex = "Male",
                DateBirth = new DateOnly(2023, 11, 3),
                DateRegistered = new DateOnly(2023, 12, 10),
                DateOnCph = new DateOnly(2023, 12, 10),
                Breed = "Angus",
                BreedCode = "ANG",
                BreedName = "Aberdeen Angus",
                State = "Alive",
                RestrictionStatus = "None",
                CurrentLocation = new() { Identifier = "23/456/7890", SubIdentifier = "02", FullIdentifier = "23/456/7890/02", Name = "Example Holdings", PremisesType = "Farm" },
                RegisteredLocation = new() { Identifier = "23/456/7890", SubIdentifier = "02", FullIdentifier = "23/456/7890/02", Name = "Example Holdings", PremisesType = "Farm" },
                DeathLocation = new() { Identifier = "98/765/4321", SubIdentifier = "01", FullIdentifier = "98/765/4321/01", Name = "Fake Abattoir", PremisesType = "Abattoir" },
                DamType = "Surrogate",
                GeneticDamEarTag = "UK12345678810",
                SurrogateDamEarTag = "UK12345678811",
                SireEarTag = "UK12345678710",
                SireName = "Fake Sire Two"
            }
        };

    public static ProblemDetails CattleNotFoundProblemDetails() =>
        NotFoundProblemDetails("Cattle with the specified ear tag was not found.", "/v1/cattle/{earTag}");

    public static ProblemDetails NotFoundProblemDetails(string detail, string instance) =>
        new()
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            Title = "Not Found",
            Status = (int)HttpStatusCode.NotFound,
            Detail = detail,
            Instance = instance
        };

    public static ProblemDetails BadRequestProblemDetails(string detail, string instance) =>
        new()
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "Bad Request",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = detail,
            Instance = instance
        };

    public static ProblemDetails ConflictProblemDetails(string detail, string instance) =>
        new()
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
            Title = "Conflict",
            Status = (int)HttpStatusCode.Conflict,
            Detail = detail,
            Instance = instance
        };

    public static IReadOnlyDictionary<string, HoldingDetailedResponse> GetHoldingsByCph() =>
        new Dictionary<string, HoldingDetailedResponse>(StringComparer.OrdinalIgnoreCase)
        {
            ["123456789"] = new()
            {
                Cph = "12/345/6789",
                Name = "Fake Farm",
                HoldingType = "Farm",
                Address = ["1 Fake Lane", "Faketown", "FA1 1KE"],
                KeeperName = "Alex Keeper",
                AllowedSpecies = ["Cattle"],
                HerdMarks =
                [
                    new()
                    {
                        Mark = "AB1234",
                        StartDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        EndDate = new DateTimeOffset(2030, 12, 31, 0, 0, 0, TimeSpan.Zero),
                        Species = ["Cattle"]
                    }
                ],
                Associations =
                [
                    new()
                    {
                        CustomerNumber = "C0001",
                        Title = "Mr",
                        FirstName = "Alex",
                        LastName = "Keeper",
                        Name = "Alex Keeper",
                        PartyType = "Person",
                        Email = "alex.keeper@example.com",
                        Mobile = "07700 900001",
                        Telephone = "01632 960001",
                        Address = ["1 Fake Lane", "Faketown", "FA1 1KE"],
                        Roles = [new() { Code = "KEEPER", Species = ["Cattle"] }]
                    }
                ]
            },
            ["234567890"] = new()
            {
                Cph = "23/456/7890",
                Name = "Example Holdings",
                HoldingType = "Farm",
                Address = ["2 Sample Road", "Exampleton", "EX2 2MP"],
                KeeperName = "Sam Farmer",
                AllowedSpecies = ["Cattle", "Sheep"],
                HerdMarks =
                [
                    new()
                    {
                        Mark = "CD5678",
                        StartDate = new DateTimeOffset(2021, 6, 1, 0, 0, 0, TimeSpan.Zero),
                        EndDate = new DateTimeOffset(2031, 5, 31, 0, 0, 0, TimeSpan.Zero),
                        Species = ["Cattle", "Sheep"]
                    }
                ],
                Associations =
                [
                    new()
                    {
                        CustomerNumber = "C0002",
                        Title = "Ms",
                        FirstName = "Sam",
                        LastName = "Farmer",
                        Name = "Sam Farmer",
                        PartyType = "Person",
                        Email = "sam.farmer@example.com",
                        Mobile = "07700 900002",
                        Telephone = "01632 960002",
                        Address = ["2 Sample Road", "Exampleton", "EX2 2MP"],
                        Roles = [new() { Code = "KEEPER", Species = ["Cattle", "Sheep"] }]
                    }
                ]
            }
        };

    public static HoldingResponse ToHoldingResponse(HoldingDetailedResponse holding) =>
        new()
        {
            Cph = holding.Cph,
            Name = holding.Name,
            HoldingType = holding.HoldingType,
            Address = holding.Address,
            KeeperName = holding.KeeperName
        };

    public static IReadOnlyList<CattleResponse> GetCattleForHolding(string normalisedCph) =>
        normalisedCph == "123456789"
            ?
            [
                new()
                {
                    EarTag = "UK12345678901",
                    DateBirth = new DateOnly(2024, 1, 15),
                    DateOnCph = new DateOnly(2024, 2, 1),
                    Sex = "Female",
                    Breed = "Holstein",
                    BreedCode = "HOL",
                    BreedName = "Holstein Friesian",
                    Status = "Alive"
                },
                new()
                {
                    EarTag = "UK12345678902",
                    DateBirth = new DateOnly(2023, 11, 3),
                    DateOnCph = new DateOnly(2023, 12, 10),
                    Sex = "Male",
                    Breed = "Angus",
                    BreedCode = "ANG",
                    BreedName = "Aberdeen Angus",
                    Status = "Alive"
                }
            ]
            : [];

    public static readonly Guid SeededBundleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static IReadOnlyList<BundleResponse> GetBundlesForHolding(string normalisedCph) =>
        normalisedCph == "123456789"
            ? [CreateBundle("FAKE-REF-001", "12/345/6789", "Alex Keeper", bundleId: SeededBundleId)]
            : [];

    public static BundleResponse CreateBundle(
        string clientReference,
        string countyParishHolding,
        string submittedBy,
        IEnumerable<AnimalRegistrationRequest>? animals = null,
        Guid? bundleId = null)
    {
        var id = bundleId ?? Guid.NewGuid();
        return new BundleResponse
        {
            Id = id,
            ClientReference = clientReference,
            CountyParishHolding = countyParishHolding,
            SubmittedBy = submittedBy,
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow,
            Animals = (animals ?? [new AnimalRegistrationRequest { EarTag = "UK12345678903", Sex = "Female", Breed = "Holstein" }])
                .Select(a => new BundleAnimalResponse
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = id,
                    Status = "Pending",
                    EarTag = a.EarTag,
                    DateBirth = a.DateOfBirth,
                    Sex = a.Sex,
                    Breed = a.Breed,
                    DamType = a.Dam?.Type,
                    DamGeneticEarTag = a.Dam?.GeneticDamEarTag,
                    DamSurrogateEarTag = a.Dam?.SurrogateDamEarTag,
                    SireEarTag = a.Sire?.EarTag,
                    SireName = a.Sire?.Name
                })
                .ToList()
        };
    }

    public static SubmissionValidationResult CreateValidationResult(BundleResponse bundle) =>
        new()
        {
            SubmissionId = bundle.Id,
            IsValid = true,
            Status = "Valid",
            ErrorCount = 0,
            AnimalResults = [.. bundle.Animals.Select(a => new SubmissionAnimalValidationResult
            {
                AnimalId = a.Id,
                EarTag = a.EarTag,
                IsValid = true
            })]
        };

    public static IReadOnlyDictionary<string, UserDetailsResponse> GetUsersBySubject() =>
        new Dictionary<string, UserDetailsResponse>(StringComparer.OrdinalIgnoreCase)
        {
            ["fake-subject-1"] = new()
            {
                Subject = "fake-subject-1",
                Email = "fake.subject@example.com",
                FirstName = "Fake",
                LastName = "Subject",
                DisplayName = "Fake Subject",
                Cphs = [new() { Cph = "12/345/6789", HoldingName = "Fake Farm", Role = "Keeper" }]
            }
        };

    public static HealthCheckResponse HealthyResponse() =>
        new()
        {
            Status = "Healthy",
            TotalDurationSeconds = 0.01,
            Entries = new Dictionary<string, ComponentHealthResult>
            {
                ["fake"] = new() { Status = "Healthy", DurationSeconds = 0.01 }
            }
        };
}