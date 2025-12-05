using System.Net.Http.Json;
using ApiTest.Data;
using Share.Models.Auth;
using SystemMod.Models;

namespace ApiTest;

public class IntegrationTest1
{
    [ClassDataSource<HttpClientDataClass>]
    [Test]
    public async Task LoginResult(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var loginDto = new SystemLoginDto
        {
            Email = "admin@default.com",
            Password = "Perigon.2026"
        };
        var response = await httpClient.PostAsJsonAsync("/api/systemUser/authorize", loginDto);
        var resData = await response.Content.ReadFromJsonAsync<AccessTokenDto>();
        await Assert.That(resData.AccessToken).IsNotNullOrEmpty();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

    }
}
