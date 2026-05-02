using System.Security.Claims;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumService.ForumService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;
        private readonly IPermissionService _permissionService;

        public ForumController(IForumService forumService, IPermissionService permissionService)
        {
            _forumService = forumService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ForumDto>>> GetAllForums(CancellationToken cancellationToken)
        {
            var forums = await _forumService.GetForums(cancellationToken);
            return Ok(forums);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateForum([FromBody] ForumDto forumDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var createdForum = await _forumService.AddForum(forumDto, userId);

            if (createdForum == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create forum");

            return CreatedAtAction(nameof(GetForum), new { forumName = createdForum.Name }, createdForum);
        }

        [Authorize]
        [HttpPost("join/{forumId}")]
        public async Task<ActionResult> JoinForum(string forumId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!Guid.TryParse(forumId, out var forumGuidParsed))
                return BadRequest("Invalid forum ID");

            await _forumService.AddUserToForum(forumGuidParsed, Guid.Parse(userId));
            return Ok();
        }

        [HttpGet("{forumName}")]
        public async Task<ActionResult<ForumDto>> GetForum(string forumName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(forumName))
                return BadRequest("Invalid forum name");

            ForumDto? forum = await _forumService.GetForum(forumName, cancellationToken);
            return forum == null ? NotFound() : forum;
        }

        [Authorize]
        [HttpGet("{forumId}/permissions")]
        public async Task<ActionResult<MemberPermissionDto?>> GetUserPermissions(string forumId, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(forumId, out var forumGuid))
                return BadRequest("Invalid forum ID");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized("Invalid user ID");

            var permissions = await _forumService.GetUserPermission(forumGuid, userGuid, cancellationToken);
            return Ok(permissions);
        }

        [AllowAnonymous]
        [HttpGet("{forumId:guid}/tags")]
        public async Task<ActionResult<List<ForumTagDto>>> GetForumTags(Guid forumId, CancellationToken cancellationToken)
        {
            try
            {
                var tags = await _forumService.GetForumTagsAsync(forumId, cancellationToken);
                return Ok(tags);
            }
            catch (ForumNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPost("{forumId:guid}/tags")]
        public async Task<ActionResult<ForumTagDto>> CreateForumTag(Guid forumId, [FromBody] CreateForumTagRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized();

            try
            {
                var created = await _forumService.CreateForumTagAsync(forumId, request, userGuid);
                return Ok(created);
            }
            catch (ForumNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("joined")]
        public async Task<ActionResult<List<ForumUserDto>>> GetAllJoinedForums(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var forums = await _forumService.GetJoinedForums(Guid.Parse(userId), cancellationToken);
            return forums;
        }

        // --- Member management endpoints ---

        [Authorize]
        [HttpGet("{forumId}/members")]
        public async Task<ActionResult<List<ForumMemberDto>>> GetForumMembers(Guid forumId, CancellationToken cancellationToken)
        {
            var members = await _forumService.GetForumMembers(forumId, cancellationToken);
            return Ok(members);
        }

        [Authorize]
        [HttpGet("{forumId}/members/{targetUserId}/permissions")]
        public async Task<ActionResult<MemberPermissionDto?>> GetMemberPermissions(Guid forumId, Guid targetUserId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized();

            var isSelf = userGuid == targetUserId;
            if (!isSelf)
            {
                var hasPermission = await _permissionService.HasPermissionAsync(
                    forumId, userGuid, Application.Enums.ForumPermissionType.ManageRoles, cancellationToken);
                if (!hasPermission)
                    return Forbid();
            }

            var permissions = await _forumService.GetUserPermission(forumId, targetUserId, cancellationToken);
            return Ok(permissions);
        }

        [Authorize]
        [HttpPut("{forumId}/members/{targetUserId}/role")]
        public async Task<ActionResult> SetMemberRole(Guid forumId, Guid targetUserId, [FromBody] SetRoleRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized();

            try
            {
                await _permissionService.SetUserRoleAsync(
                    new ForumMemberRoleUpdate(forumId, targetUserId, request.Role, userGuid));
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("{forumId}/members/{targetUserId}/permissions")]
        public async Task<ActionResult> SetMemberPermissions(
            Guid forumId, Guid targetUserId, [FromBody] SetPermissionOverridesRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized();

            try
            {
                await _permissionService.SetPermissionOverridesAsync(
                    new ForumMemberPermissionOverridesUpdate(
                        forumId, targetUserId, request.Permissions, userGuid));
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{forumId}/members/{targetUserId}")]
        public async Task<ActionResult> RemoveMember(Guid forumId, Guid targetUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized();

            try
            {
                await _forumService.RemoveForumMember(forumId, targetUserId, userGuid);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
