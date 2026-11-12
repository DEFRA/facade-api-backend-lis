using System.Net;
using LisFacadeApiBackend.Facade.Cattle.Request;

namespace LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

public sealed class CattleApiClient(HttpClient httpClient, ILogger<CattleApiClient> logger) : ICattleApiClient
{
    public Task<HttpResponseMessage> GetCattleDetailsAsync(
        string earTag,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(earTag);
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogDebug("Calling cattle API v1/cattle/ for {CattleIdentifier}", earTag);
        return SendGetCattleDetailsAsync(earTag, cancellationToken);
    }

    private Task<HttpResponseMessage> SendGetCattleDetailsAsync(string earTag, CancellationToken cancellationToken) =>
        SendGetAsync($"v1/cattle/{Uri.EscapeDataString(earTag)}", cancellationToken);

    public Task<HttpResponseMessage> SearchHoldingsAsync(
        string? search, int? page, int? pageSize, string? sort, string? order,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetHoldingAsync(
        string county, string parish, string holding, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetCattleForHoldingMultiSegmentAsync(
        string county, string parish, string holding, string? earTag, string? breed, string? sex,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(county);
        ArgumentException.ThrowIfNullOrWhiteSpace(parish);
        ArgumentException.ThrowIfNullOrWhiteSpace(holding);
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogDebug($"Calling cattle API GET /v1/holdings/{{county}}/{{parish}}/{{holding}}/cattle " + 
            $"for county={county}, parish={parish}, holding={holding}");
            
        return SendGetAsync($"v1/holdings/{Uri.EscapeDataString(county)}/{Uri.EscapeDataString(parish)}/"
            + $"{Uri.EscapeDataString(holding)}/cattle?earTag={Uri.EscapeDataString(earTag ?? "")}&breed={Uri.EscapeDataString(breed ?? "")}"
            + $"&sex={Uri.EscapeDataString(sex ?? "")}", cancellationToken);
    }

    public Task<HttpResponseMessage> GetCattleForHoldingAsync(
        string cph, string? earTag, string? breed, string? sex, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetBundlesForHoldingMultiSegmentAsync(
        string county, string parish, string holding, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetBundlesForHoldingAsync(
        string cph, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> CreateRegistrationBundleAsync(
        RegistrationBundleRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> ValidateRegistrationBundleAsync(
        string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetUserDetailsAsync(
        string subject, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> CreateUserAsync(
        CreateUserRequest? request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetDetailedHealthAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    private async Task<HttpResponseMessage> SendGetAsync(string requestUri, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetAsync(requestUri, cancellationToken);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new HttpRequestException("Cattle API request timed out", ex, HttpStatusCode.GatewayTimeout);
        }
    }
}
