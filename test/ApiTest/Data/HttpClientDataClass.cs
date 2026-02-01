using System.Net.Http.Json;
using Share.Models.Auth;
using TUnit.Core.Interfaces;

namespace ApiTest.Data;

public class HttpClientDataClass : IAsyncInitializer, IAsyncDisposable
{
    public HttpClient HttpClient { get; private set; } = new();
    
    public async Task InitializeAsync()
    {
        HttpClient = (GlobalHooks.App ?? throw new NullReferenceException()).CreateHttpClient("AdminService");
        if (GlobalHooks.NotificationService != null)
        {
            await GlobalHooks.NotificationService.WaitForResourceAsync("AdminService", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        }

        // 自动登录获取 token
        await AuthenticateAsync();
    }

    private async Task AuthenticateAsync()
    {
        try
        {
            var loginDto = new
            {
                UserName = "admin",
                Password = "Perigon.2026"
            };

            var response = await HttpClient.PostAsJsonAsync("/api/systemUser/login", loginDto);
            if (response.IsSuccessStatusCode)
            {
                var tokenResult = await response.Content.ReadFromJsonAsync<AccessTokenDto>();
                if (tokenResult?.AccessToken != null)
                {
                    HttpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);
                }
            }
        }
        catch
        {
            // 登录失败时继续,让测试自行处理
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Console.Out.WriteLineAsync("And when the class is finished with, we can clean up any resources.");
    }
}
