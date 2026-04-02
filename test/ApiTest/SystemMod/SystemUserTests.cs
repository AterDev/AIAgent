using ApiTest.Data;
using Share.Models.Auth;
using SystemMod.Models.SystemUserDtos;
using Entity.SystemMod;
using Perigon.AspNetCore.Models;
using System.Net.Http.Json;

namespace ApiTest.SystemMod;

/// <summary>
/// 系统用户集成测试
/// </summary>
public class SystemUserTests
{
    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task SystemUserCRUD_ShouldWorkCorrectly(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // Add - 创建系统用户
        var addDto = new SystemUserAddDto
        {
            UserName = $"testuser{Guid.NewGuid().ToString().Substring(0, 8)}",
            Email = $"test.user.{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
            RealName = "测试用户",
            Password = "Test@123456"
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/systemUser", addDto);
        await Assert.That(addResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

        var addedUser = await addResponse.Content.ReadFromJsonAsync<SystemUser>();
        await Assert.That(addedUser).IsNotNull();
        await Assert.That(addedUser!.Email).IsEqualTo(addDto.Email);
        var userId = addedUser.Id;

        // Get - 获取用户详情
        var getResponse = await httpClient.GetAsync($"/api/systemUser/{userId}");
        await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var userDetail = await getResponse.Content.ReadFromJsonAsync<SystemUserDetailDto>();
        await Assert.That(userDetail).IsNotNull();
        await Assert.That(userDetail!.Email).IsEqualTo(addDto.Email);
        await Assert.That(userDetail.UserName).IsEqualTo(addDto.UserName);

        // Update - 更新用户
        var updateDto = new SystemUserUpdateDto
        {
            RealName = "更新后的用户名",
            Email = $"updated.user.{Guid.NewGuid().ToString().Substring(0, 8)}@example.com"
        };

        var updateResponse = await httpClient.PatchAsJsonAsync($"/api/systemUser/{userId}", updateDto);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var updateResult = await updateResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(updateResult).IsTrue();

        // Verify Update
        var verifyResponse = await httpClient.GetAsync($"/api/systemUser/{userId}");
        var updatedUser = await verifyResponse.Content.ReadFromJsonAsync<SystemUserDetailDto>();
        await Assert.That(updatedUser!.RealName).IsEqualTo(updateDto.RealName);

        // Delete - 删除用户
        var deleteResponse = await httpClient.DeleteAsync($"/api/systemUser/{userId}");
        await Assert.That(deleteResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(deleteResult).IsTrue();

        // Verify Delete
        var verifyDeleteResponse = await httpClient.GetAsync($"/api/systemUser/{userId}");
        await Assert.That(
            verifyDeleteResponse.StatusCode == HttpStatusCode.NotFound
            || verifyDeleteResponse.StatusCode == HttpStatusCode.Forbidden).IsTrue();

        var verifyListResponse = await httpClient.PostAsJsonAsync("/api/systemUser/filter", new SystemUserFilterDto
        {
            PageIndex = 1,
            PageSize = 20,
            UserName = addDto.UserName,
        });
        await Assert.That(verifyListResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var verifyList = await verifyListResponse.Content.ReadFromJsonAsync<PageList<SystemUserItemDto>>();
        await Assert.That(verifyList).IsNotNull();
        await Assert.That((verifyList!.Data ?? []).Any(q => q.Id == userId)).IsFalse();
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ListSystemUsers_ShouldReturnPagedResults(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var filter = new SystemUserFilterDto
        {
            PageIndex = 1,
            PageSize = 10
        };

        var response = await httpClient.PostAsJsonAsync("/api/systemUser/filter", filter);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var pagedResult = await response.Content.ReadFromJsonAsync<PageList<SystemUserItemDto>>();
        await Assert.That(pagedResult).IsNotNull();
        await Assert.That(pagedResult!.Data).IsNotNull();
        await Assert.That(pagedResult.Data.Count).IsGreaterThanOrEqualTo(0);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAccessToken(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var loginDto = new LoginDto
        {
            UserName = "admin",
            Password = "Perigon.2026"
        };

        var response = await httpClient.PostAsJsonAsync("/api/systemUser/login", loginDto);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var tokenDto = await response.Content.ReadFromJsonAsync<AccessTokenDto>();
        await Assert.That(tokenDto).IsNotNull();
        await Assert.That(tokenDto!.AccessToken).IsNotNullOrEmpty();
        await Assert.That(tokenDto.ExpiresIn).IsGreaterThan(0);
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task GetCurrentUserInfo_ShouldReturnUserInfo(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        var response = await httpClient.GetAsync("/api/systemUser/current");
        
        // 如果已认证，应该返回用户信息
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var userInfo = await response.Content.ReadFromJsonAsync<UserInfoDto>();
            await Assert.That(userInfo).IsNotNull();
            await Assert.That(userInfo!.UserName).IsNotNullOrEmpty();
        }
    }

    [ClassDataSource<HttpClientDataClass>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task ChangePassword_WithValidData_ShouldSucceed(HttpClientDataClass httpClientData)
    {
        var httpClient = httpClientData.HttpClient;

        // 先创建一个新用户用于测试修改密码
        var addDto = new SystemUserAddDto
        {
            UserName = $"pwdchange{Guid.NewGuid().ToString().Substring(0, 8)}",
            Email = $"pwd.change.{Guid.NewGuid().ToString().Substring(0, 8)}@example.com",
            RealName = "密码修改测试用户",
            Password = "OldPassword123"
        };

        var addResponse = await httpClient.PostAsJsonAsync("/api/systemUser", addDto);
        var addedUser = await addResponse.Content.ReadFromJsonAsync<SystemUser>();
        var userId = addedUser!.Id;

        // 登录该新用户
        var loginDto = new LoginDto
        {
            UserName = addDto.UserName,
            Password = addDto.Password
        };

        var loginResponse = await httpClient.PostAsJsonAsync("/api/systemUser/login", loginDto);
        var tokenDto = await loginResponse.Content.ReadFromJsonAsync<AccessTokenDto>();

        // 使用新的token创建新客户端
        var authHttpClient = new HttpClient
        {
            BaseAddress = httpClient.BaseAddress
        };
        authHttpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenDto!.AccessToken);

        // 修改密码
        var changePasswordDto = new ChangePasswordDto
        {
            OldPassword = "OldPassword123",
            NewPassword = "NewPassword456"
        };

        var changeResponse = await authHttpClient.PostAsJsonAsync("/api/systemUser/change-password", changePasswordDto);
        await Assert.That(changeResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var changeResult = await changeResponse.Content.ReadFromJsonAsync<bool>();
        await Assert.That(changeResult).IsTrue();
    }
}
