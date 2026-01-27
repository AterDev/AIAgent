using ModelMod.Models.ModelProfileDtos;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 模型配置管理
/// </summary>
public class ModelProfileController(
    Localizer localizer,
    IUserContext user,
    ILogger<ModelProfileController> logger,
    ModelProfileManager manager
) : RestControllerBase<ModelProfileManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ModelProfileItemDto>>> ListAsync(ModelProfileFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ModelProfile>> AddAsync(ModelProfileAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ModelProfileUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<ModelProfileDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
