using KnowledgeBaseMod.Managers;
using KnowledgeBaseMod.Models.RagCollectionDtos;
using ModelMod.Managers;
using ModelMod.Models.ApplicationRagCollectionPermissionDtos;

namespace ApiService.Controllers.OpenPlatform;

/// <summary>
/// Open platform rag collections
/// </summary>
[ApiController]
[Route("api/v1/rag/collections")]
public class RagCollectionsController(
    RagCollectionManager manager,
    ApplicationRagCollectionPermissionManager linkManager,
    IUserContext user,
    ILogger<RagCollectionsController> logger
) : OpenApiControllerBase<RagCollectionManager>(manager, user, logger)
{
    private readonly ApplicationRagCollectionPermissionManager _linkManager = linkManager;

    [HttpPost("filter")]
    public async Task<ActionResult<PageList<RagCollectionItemDto>>> ListAsync(RagCollectionFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    [HttpPost]
    public async Task<ActionResult<RagCollection>> AddAsync(RagCollectionAddDto dto)
    {
        if (_user.IsRole(WebConst.Application))
        {
            dto.ApplicationId = _user.UserId;
        }

        var entity = await _manager.AddAsync(dto);
        return CreatedAtRoute(null, new { id = entity.Id }, entity);
    }

    [HttpGet("{id}")]
    public async Task<RagCollectionDetailDto?> DetailAsync([FromRoute] Guid id)
    {
        return await _manager.GetAsync(id);
    }

    [HttpPatch("{id}")]
    public async Task<bool> UpdateAsync([FromRoute] Guid id, RagCollectionUpdateDto dto)
    {
        return await _manager.EditAsync(id, dto) == 1;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        if (_user.IsRole(WebConst.Application))
        {
            var links = await _linkManager.FilterAsync(new ApplicationRagCollectionPermissionFilterDto
            {
                ApplicationId = _user.UserId,
                RagCollectionId = id,
                PageIndex = 1,
                PageSize = 100,
            });

            if (links.Data.Count == 0)
            {
                return Ok(false);
            }

            await _linkManager.DeleteAsync([.. links.Data.Select(q => q.Id)], false);
            return Ok(true);
        }

        return await _manager.DeleteAsync([id], false);
    }
}