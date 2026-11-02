using System.Net;
using System.Net.Http.Json;
using LisFacadeApiBackend.Facade.Cattle.Error;
using LisFacadeApiBackend.Facade.Cattle.Request;
using LisFacadeApiBackend.Facade.Cattle.Response.Users;
using LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.ClientFakes;

public class CreateUserAsyncUnitTests
{
    [Fact]
    public async Task Returns_ok_with_new_user()
    {
        var client = new FakeCattleApiClient();
        var request = new CreateUserRequest { Sub = "new-sub", Email = "new@example.com", GivenName = "New", FamilyName = "User" };

        using var response = await client.CreateUserAsync(request, CancellationToken.None);
        var user = await response.Content.ReadFromJsonAsync<UserDetailsResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("new-sub", user!.Subject);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("New", user.FirstName);
        Assert.Equal("User", user.LastName);
        Assert.Equal("New User", user.DisplayName);
    }

    [Fact]
    public async Task Created_user_can_subsequently_be_retrieved()
    {
        var client = new FakeCattleApiClient();
        var request = new CreateUserRequest { Sub = "new-sub", Email = "new@example.com" };

        using var createResponse = await client.CreateUserAsync(request, CancellationToken.None);
        using var getResponse = await client.GetUserDetailsAsync("new-sub", CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Theory]
    [InlineData("fake-subject-1")]
    [InlineData("FAKE-SUBJECT-1")]
    public async Task Returns_conflict_when_subject_already_exists(string sub)
    {
        var client = new FakeCattleApiClient();
        var request = new CreateUserRequest { Sub = sub, Email = "dupe@example.com" };

        using var response = await client.CreateUserAsync(request, CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(409, problem!.Status);
    }

    [Fact]
    public async Task Returns_conflict_when_creating_same_user_twice()
    {
        var client = new FakeCattleApiClient();
        var request = new CreateUserRequest { Sub = "new-sub", Email = "new@example.com" };

        using var first = await client.CreateUserAsync(request, CancellationToken.None);
        using var second = await client.CreateUserAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Returns_bad_request_for_null_request()
    {
        var client = new FakeCattleApiClient();

        using var response = await client.CreateUserAsync(null, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData(null, "a@example.com")]
    [InlineData("", "a@example.com")]
    [InlineData(" ", "a@example.com")]
    [InlineData("sub", null)]
    [InlineData("sub", "")]
    [InlineData("sub", " ")]
    public async Task Returns_bad_request_when_subject_or_email_missing(string? sub, string? email)
    {
        var client = new FakeCattleApiClient();

        using var response = await client.CreateUserAsync(new CreateUserRequest { Sub = sub, Email = email }, CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, problem!.Status);
    }

    [Fact]
    public async Task Throws_operation_canceled_when_token_already_cancelled()
    {
        var client = new FakeCattleApiClient();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => client.CreateUserAsync(new CreateUserRequest { Sub = "s", Email = "e" }, cts.Token));
    }
}
