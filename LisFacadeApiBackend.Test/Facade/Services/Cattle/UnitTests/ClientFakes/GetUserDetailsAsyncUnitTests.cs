using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Response.Users;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class GetUserDetailsAsyncUnitTests
{
    [Theory]
    [InlineData("fake-subject-1")]
    [InlineData("FAKE-SUBJECT-1")]
    public async Task Returns_ok_with_user_matching_subject_ignoring_case(string subject)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetUserDetailsAsync(subject, CancellationToken.None);
        var user = await response.Content.ReadFromJsonAsync<UserDetailsResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("fake-subject-1", user!.Subject);
        Assert.Equal("fake.subject@example.com", user.Email);
        Assert.Equal("Fake Subject", user.DisplayName);
        Assert.Single(user.Cphs);
    }

    [Fact]
    public async Task Returns_not_found_for_unknown_subject()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetUserDetailsAsync("unknown", CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, problem!.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Returns_bad_request_for_null_empty_or_whitespace_subject(string? subject)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.GetUserDetailsAsync(subject!, CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(400, problem!.Status);
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.GetUserDetailsAsync("fake-subject-1", cts.Token));
    }
}
