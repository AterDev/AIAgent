using ModelMod.Models.ModelProviderDtos;

namespace AdminService.Controllers.ModelMod;

/// <summary>
/// 模型提供商管理
/// </summary>
public class ModelProviderController(
    Localizer localizer,
    IUserContext user,
    ILogger<ModelProviderController> logger,
    ModelProviderManager manager
) : RestControllerBase<ModelProviderManager>(localizer, manager, user, logger)
{
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ModelProviderItemDto>>> ListAsync(ModelProviderFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<ModelProvider>> AddAsync(ModelProviderAddDto dto)
    {
        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, ModelProviderUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpGet("{id}")]
    public async Task<ModelProviderDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }
}
