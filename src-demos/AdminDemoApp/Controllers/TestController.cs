using Microsoft.AspNetCore.Mvc;

namespace AdminDemoApp.Controllers;

public class TestController : RestControllerBase<SystemConfigManager>
{
    public TestController(Localizer localizer,
        SystemConfigManager manager,
        IUserContext user,
        ILogger<TestController> logger
    )
        : base(localizer, manager, user, logger)
    {

    }

    public IActionResult Index()
    {
        return Content("test");
    }

    /// <summary>
    /// 测试生成类
    /// </summary>
    /// <returns></returns>
    public IActionResult Code()
    {
        return Content("code");
    }


    /// <summary>
    /// 获取枚举信息 ✅
    /// </summary>
    /// <returns></returns>
    [HttpGet("enum")]
    public async Task<ActionResult<Dictionary<string, List<EnumDictionary>>>> GetEnumConfigsAsync()
    {
        return await _manager.GetEnumConfigsAsync();
    }
}
