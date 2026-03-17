using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Config.Api.ViewModels.Parameters;
using SA.Config.Domain.Enums;
using SA.Config.Tests.E2E.Fixtures;
using Shared.Contract.Models;
using Xunit;

namespace SA.Config.Tests.E2E.Parameters;

public sealed class ParameterEndpointTests : IClassFixture<ConfigWebAppFactory>
{
    private readonly ConfigWebAppFactory _factory;

    public ParameterEndpointTests(ConfigWebAppFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateAuthenticatedClient(ConfigWebAppFactory? factory = null)
    {
        var f = factory ?? _factory;
        var client = f.CreateClient();
        var token = f.GenerateJwtToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<Guid> GetFirstGroupIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/parameter-groups");
        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        return categories!.First().Groups.First().Id;
    }

    [Fact]
    public async Task Create_Parameter_Returns_201()
    {
        // Arrange
        using var factory = new ConfigWebAppFactory();
        var client = CreateAuthenticatedClient(factory);
        var groupId = await GetFirstGroupIdAsync(client);

        var request = new CreateParameterRequest(
            groupId,
            "test_param_key",
            "test_value",
            ParameterType.Text,
            "A test parameter",
            null,
            null);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/parameters", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ParameterResponse>();
        body.Should().NotBeNull();
        body!.Key.Should().Be("test_param_key");
        body.Value.Should().Be("test_value");
        body.Type.Should().Be(ParameterType.Text);
    }

    [Fact]
    public async Task Get_Parameters_Returns_List()
    {
        // Arrange
        using var factory = new ConfigWebAppFactory();
        var client = CreateAuthenticatedClient(factory);
        var groupId = await GetFirstGroupIdAsync(client);

        // Create a parameter first
        var createRequest = new CreateParameterRequest(
            groupId, "list_param", "val", ParameterType.Text, "Desc", null, null);
        await client.PostAsJsonAsync("/api/v1/parameters", createRequest);

        // Act
        var response = await client.GetAsync($"/api/v1/parameters?group_id={groupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ParameterResponse>>();
        body.Should().NotBeNull();
        body!.Should().Contain(p => p.Key == "list_param");
    }

    [Fact]
    public async Task Update_Parameter_Returns_200()
    {
        // Arrange
        using var factory = new ConfigWebAppFactory();
        var client = CreateAuthenticatedClient(factory);
        var groupId = await GetFirstGroupIdAsync(client);

        var createRequest = new CreateParameterRequest(
            groupId, "update_param", "old_value", ParameterType.Text, "Desc", null, null);
        var createResponse = await client.PostAsJsonAsync("/api/v1/parameters", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ParameterResponse>();

        var updateRequest = new UpdateParameterRequest("new_value", null);

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/parameters/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ParameterResponse>();
        body.Should().NotBeNull();
        body!.Value.Should().Be("new_value");
    }

    [Fact]
    public async Task Delete_Parameter_Returns_204()
    {
        // Arrange
        using var factory = new ConfigWebAppFactory();
        var client = CreateAuthenticatedClient(factory);
        var groupId = await GetFirstGroupIdAsync(client);

        var createRequest = new CreateParameterRequest(
            groupId, "delete_param", "val", ParameterType.Text, "Desc", null, null);
        var createResponse = await client.PostAsJsonAsync("/api/v1/parameters", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ParameterResponse>();

        // Act
        var response = await client.DeleteAsync($"/api/v1/parameters/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Duplicate_Key_Returns_409()
    {
        // Arrange
        using var factory = new ConfigWebAppFactory();
        var client = CreateAuthenticatedClient(factory);
        var groupId = await GetFirstGroupIdAsync(client);

        var request = new CreateParameterRequest(
            groupId, "dup_key", "val", ParameterType.Text, "Desc", null, null);

        // Create first
        var first = await client.PostAsJsonAsync("/api/v1/parameters", request);
        first.EnsureSuccessStatusCode();

        // Act - try to create duplicate
        var response = await client.PostAsJsonAsync("/api/v1/parameters", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
