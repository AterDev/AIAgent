---
name: test
description: 编写Asp.Net Core测试代码的规范和最佳实践
---

## 何时使用

在编写后端测试代码时，如单元测试、集成测试及其他与测试相关的内容。

## 项目说明

- `tests\ApiTest\`：是基于`Aspire/Tunit`的测试项目，主要用来编写集成测试。
- 已统一配置了`TestHttpClientData`，用来进行请求授权，使用`[ClassDataSource<TestHttpClientData>(Shared = SharedType.PerTestSession)]` 特性标记，以避免每次请求都需要重新获取Token。

## 规范

- 以功能模块创建对应的目录，例如`tests\ApiTest\{ModuleName}\`，将该模块的所有测试代码放在该目录下。
- 以实体创建对应的测试类文件，例如`{EntityName}Tests.cs`，将该实体相关的所有测试方法放在该文件中。
- 通常一个实体对应一个控制器，测试内容为接口中的功能测试。
- 注意测试运行的隔离性，确保每个测试方法独立运行，不依赖其他测试方法的执行结果。
- 使用有意义且风格统一的测试方法名称，清晰描述测试目的和预期结果。
- 使用`TUnit`提供的断言方法进行结果验证，确保测试结果符合预期。

## 编写测试方法的建议

- 根据控制器接口编写测试方法，使用接近真实场景的数据进行测试。
- 进行适当的合并，比如基本的增删改查，可以放在一个测试方法中完成，避免过多的测试方法导致测试类臃肿。其他方法用来测试特殊场景或边界情况。
- 优先编写集成测试而非单元测试，通过完整的 API 端点进行测试。
- 优先测试正常场景，确保主要功能正常运行后，再考虑异常和边界情况的测试。
- 无需专门测试BadRequest。

## 流程

1. 创建测试类文件(如果没有)，例如`SystemUserTests.cs`，放在对应的模块目录下。
2. 分析控制器接口，确定需要测试的方法。
3. 根据接口内容，添加/移除/更新测试方法，确保覆盖主要功能。
4. 编写测试逻辑，并自我检查逻辑。
5. build测试项目，检查逻辑和构建问题。
6. 按功能模块依次运行测试内容。
7. 创建或更新测试结果文档，`docs\test.result.md`，按功能模块来描述测试的结果情况。

## 其他说明

无需清理数据，因为使用aspire运行测试结束时，会自动清理库数据。

## 示例代码

```csharp
public class SystemUserTests
{
    [ClassDataSource<TestHttpClientData>(Shared = SharedType.PerTestSession)]
    [Test]
    public async Task GetUserInfo_ShouldReturnUserDetails(TestHttpClientData httpClientData)
    {
        var httpClient = httpClientData.HttpClient;
        var response = await httpClient.GetAsync("/api/systemUser/userinfo");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var userInfo = await response.Content.ReadFromJsonAsync<UserInfoDto>();
        await Assert.That(userInfo).IsNotNull();
        await Assert.That(userInfo!.Username).IsNotNullOrEmpty();
    }
}
```
