using System.Net;
using System.Text;
using System.Collections.Concurrent;
using System.Text.Json;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Bundle;
using LisFacadeApiBackend.Facade.Cattle.Response.Holding;
using LisFacadeApiBackend.Facade.Cattle.Response.Users;

namespace LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

public sealed class FakeCattleApiClient : ICattleApiClient
{
    private readonly ConcurrentDictionary<Guid, BundleResponse> _bundles = new(
        FakeCattleApiData.GetBundlesForHolding("123456789")
            .Select(b => KeyValuePair.Create(b.Id, b)));

    private readonly ConcurrentDictionary<string, UserDetailsResponse> _users = new(
        FakeCattleApiData.GetUsersBySubject(),
        StringComparer.OrdinalIgnoreCase);

    private const string JsonMediaType = "application/json";
    private const string ProblemMediaType = "application/problem+json";

    public Task<HttpResponseMessage> GetCattleDetailsAsync(
        string earTag,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(earTag);
        cancellationToken.ThrowIfCancellationRequested();

        if (FakeCattleApiData.GetCattleByEarTag().TryGetValue(earTag, out var cattle))
        {
            return Task.FromResult(CreateResponse(HttpStatusCode.OK, cattle, JsonMediaType));
        }

        return Task.FromResult(CreateResponse(
            HttpStatusCode.NotFound,
            FakeCattleApiData.CattleNotFoundProblemDetails(),
            ProblemMediaType));
    }

    public Task<HttpResponseMessage> SearchHoldingsAsync(
        string? search,
        int? page,
        int? pageSize,
        string? sort,
        string? order,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? 10;
        if (currentPage < 1 || currentPageSize < 1)
        {
            return Task.FromResult(CreateResponse(
                HttpStatusCode.BadRequest,
                FakeCattleApiData.BadRequestProblemDetails("Page and PageSize must be greater than zero.", "/v1/holdings"),
                ProblemMediaType));
        }

        var matches = FakeCattleApiData.GetHoldingsByCph().Values
            .Select(FakeCattleApiData.ToHoldingResponse)
            .Where(h => string.IsNullOrWhiteSpace(search)
                || NormaliseCph(h.Cph).Contains(NormaliseCph(search), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var sortKey = ResolveHoldingSortKey(sort);
        if (sortKey is null)
        {
            return Task.FromResult(CreateResponse(
                HttpStatusCode.BadRequest,
                FakeCattleApiData.BadRequestProblemDetails(
                    "Sort must be one of: cph, name, holdingType, keeperName.",
                    "/v1/holdings"),
                ProblemMediaType));
        }

        matches = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase)
            ? [.. matches.OrderByDescending(sortKey, StringComparer.OrdinalIgnoreCase).ThenBy(h => h.Cph)]
            : [.. matches.OrderBy(sortKey, StringComparer.OrdinalIgnoreCase).ThenBy(h => h.Cph)];

        var offset = Math.Min((currentPage - 1L) * currentPageSize, int.MaxValue);
        var items = matches.Skip((int)offset).Take(currentPageSize).ToList();
        var totalPages = (int)Math.Ceiling(matches.Count / (double)currentPageSize);

        return Task.FromResult(CreateResponse(
            HttpStatusCode.OK,
            new PaginatedResponseOfHoldingResponse
            {
                Count = items.Count,
                TotalCount = matches.Count,
                Items = items,
                Page = currentPage,
                PageSize = currentPageSize,
                TotalPages = totalPages,
                HasNextPage = currentPage < totalPages,
                HasPreviousPage = currentPage > 1
            },
            JsonMediaType));
    }

    public Task<HttpResponseMessage> GetHoldingAsync(
        string county,
        string parish,
        string holding,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(county) || string.IsNullOrWhiteSpace(parish) || string.IsNullOrWhiteSpace(holding))
        {
            return Task.FromResult(CreateResponse(
                HttpStatusCode.BadRequest,
                FakeCattleApiData.BadRequestProblemDetails(
                    "County, parish and holding are required.",
                    "/v1/holdings/{county}/{parish}/{holding}"),
                ProblemMediaType));
        }

        return Task.FromResult(FindHolding(NormaliseCph(county, parish, holding), "/v1/holdings/{county}/{parish}/{holding}"));
    }

    public Task<HttpResponseMessage> GetCattleForHoldingMultiSegmentAsync(
        string county,
        string parish,
        string holding,
        string? earTag,
        string? breed,
        string? sex,
        CancellationToken cancellationToken)
    {
        ValidateSegments(county, parish, holding);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(FindCattle(
            NormaliseCph(county, parish, holding),
            earTag,
            breed,
            sex,
            "/v1/holdings/{county}/{parish}/{holding}/cattle"));
    }

    public Task<HttpResponseMessage> GetCattleForHoldingAsync(
        string cph,
        string? earTag,
        string? breed,
        string? sex,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cph);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(FindCattle(NormaliseCph(cph), earTag, breed, sex, "/v1/holdings/{cph}/cattle"));
    }

    public Task<HttpResponseMessage> GetBundlesForHoldingMultiSegmentAsync(
        string county,
        string parish,
        string holding,
        CancellationToken cancellationToken)
    {
        ValidateSegments(county, parish, holding);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(CreateResponse(
            HttpStatusCode.OK,
            FilterBundles(NormaliseCph(county, parish, holding)),
            JsonMediaType));
    }

    public Task<HttpResponseMessage> GetBundlesForHoldingAsync(
        string cph,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cph);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(CreateResponse(
            HttpStatusCode.OK,
            FilterBundles(NormaliseCph(cph)),
            JsonMediaType));
    }

    public Task<HttpResponseMessage> CreateRegistrationBundleAsync(
        RegistrationBundleRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request is null
            || string.IsNullOrWhiteSpace(request.ClientReference)
            || string.IsNullOrWhiteSpace(request.Holding?.Cph))
        {
            return Task.FromResult(CreateResponse(
                HttpStatusCode.BadRequest,
                FakeCattleApiData.BadRequestProblemDetails("ClientReference and Holding.Cph are required.", "/v1/registrations"),
                ProblemMediaType));
        }

        var bundle = FakeCattleApiData.CreateBundle(
            request.ClientReference,
            request.Holding.Cph,
            request.SubmittedBy ?? string.Empty,
            request.Animals);

        _bundles[bundle.Id] = bundle;

        return Task.FromResult(CreateResponse(HttpStatusCode.Created, bundle, JsonMediaType));
    }

    public Task<HttpResponseMessage> ValidateRegistrationBundleAsync(
        string id,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Guid.TryParse(id, out var submissionId)
            || !_bundles.TryGetValue(submissionId, out var bundle))
        {
            return Task.FromResult(CreateResponse(
                HttpStatusCode.NotFound,
                FakeCattleApiData.NotFoundProblemDetails(
                    "Registration bundle with the specified id was not found.",
                    "/v1/registrations/{id}/validate"),
                ProblemMediaType));
        }

        return Task.FromResult(CreateResponse(
            HttpStatusCode.OK,
            FakeCattleApiData.CreateValidationResult(bundle),
            JsonMediaType));
    }

    public Task<HttpResponseMessage> GetUserDetailsAsync(
        string subject,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(subject))
        {
            return Task.FromResult(CreateResponse(
                HttpStatusCode.BadRequest,
                FakeCattleApiData.BadRequestProblemDetails("Subject is required.", "/v1/users/{subject}"),
                ProblemMediaType));
        }

        if (_users.TryGetValue(subject, out var user))
        {
            return Task.FromResult(CreateResponse(HttpStatusCode.OK, user, JsonMediaType));
        }

        return Task.FromResult(CreateResponse(
            HttpStatusCode.NotFound,
            FakeCattleApiData.NotFoundProblemDetails("User with the specified subject was not found.", "/v1/users/{subject}"),
            ProblemMediaType));
    }

    public Task<HttpResponseMessage> CreateUserAsync(
        CreateUserRequest? request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request?.Sub) || string.IsNullOrWhiteSpace(request.Email))
        {
            return Task.FromResult(CreateResponse(
                HttpStatusCode.BadRequest,
                FakeCattleApiData.BadRequestProblemDetails("Subject and email are required.", "/v1/users"),
                ProblemMediaType));
        }

        var newUser = new UserDetailsResponse
        {
            Subject = request.Sub,
            Email = request.Email,
            FirstName = request.GivenName,
            LastName = request.FamilyName,
            DisplayName = $"{request.GivenName} {request.FamilyName}".Trim()
        };

        if (!_users.TryAdd(newUser.Subject, newUser))
        {
            return Task.FromResult(CreateResponse(
                HttpStatusCode.Conflict,
                FakeCattleApiData.ConflictProblemDetails("A user with the specified subject already exists.", "/v1/users"),
                ProblemMediaType));
        }

        return Task.FromResult(CreateResponse(HttpStatusCode.OK, newUser, JsonMediaType));
    }

    public Task<HttpResponseMessage> GetDetailedHealthAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(CreateResponse(
            HttpStatusCode.OK,
            FakeCattleApiData.HealthyResponse(),
            JsonMediaType));
    }

    private static HttpResponseMessage FindHolding(string normalisedCph, string instance) =>
        FakeCattleApiData.GetHoldingsByCph().TryGetValue(normalisedCph, out var holding)
            ? CreateResponse(HttpStatusCode.OK, holding, JsonMediaType)
            : CreateResponse(
                HttpStatusCode.NotFound,
                FakeCattleApiData.NotFoundProblemDetails("Holding with the specified CPH was not found.", instance),
                ProblemMediaType);

    private static HttpResponseMessage FindCattle(
        string normalisedCph,
        string? earTag,
        string? breed,
        string? sex,
        string instance)
    {
        if (!FakeCattleApiData.GetHoldingsByCph().ContainsKey(normalisedCph))
        {
            return CreateResponse(
                HttpStatusCode.NotFound,
                FakeCattleApiData.NotFoundProblemDetails("Holding with the specified CPH was not found.", instance),
                ProblemMediaType);
        }

        var cattle = FakeCattleApiData.GetCattleForHolding(normalisedCph)
            .Where(c => string.IsNullOrWhiteSpace(earTag) || string.Equals(c.EarTag, earTag, StringComparison.OrdinalIgnoreCase))
            .Where(c => string.IsNullOrWhiteSpace(breed) || string.Equals(c.Breed, breed, StringComparison.OrdinalIgnoreCase))
            .Where(c => string.IsNullOrWhiteSpace(sex) || string.Equals(c.Sex, sex, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return CreateResponse(HttpStatusCode.OK, cattle, JsonMediaType);
    }

    private static void ValidateSegments(string county, string parish, string holding)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(county);
        ArgumentException.ThrowIfNullOrWhiteSpace(parish);
        ArgumentException.ThrowIfNullOrWhiteSpace(holding);
    }

    private List<BundleResponse> FilterBundles(string normalisedCph) =>
        [.. _bundles.Values
            .Where(b => NormaliseCph(b.CountyParishHolding) == normalisedCph)
            .OrderBy(b => b.CreatedAt)];

    private static Func<HoldingResponse, string>? ResolveHoldingSortKey(string? sort) =>
        sort?.Trim().ToLowerInvariant() switch
        {
            null or "" or "cph" => h => h.Cph,
            "name" => h => h.Name ?? string.Empty,
            "holdingtype" => h => h.HoldingType ?? string.Empty,
            "keepername" => h => h.KeeperName ?? string.Empty,
            _ => null
        };

    private static string NormaliseCph(params string[] segments) =>
        new(string.Concat(segments).Where(char.IsLetterOrDigit).ToArray());

    private static HttpResponseMessage CreateResponse<T>(HttpStatusCode statusCode, T body, string mediaType) =>
        new(statusCode)
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, mediaType)
        };
}