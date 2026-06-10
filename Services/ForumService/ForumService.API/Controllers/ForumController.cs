using System.Security.Claims;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Features.Forums.Commands.CreateForum;
using ForumService.ForumService.Application.Features.Forums.Commands.CreateForumTag;
using ForumService.ForumService.Application.Features.Forums.Commands.JoinForum;
using ForumService.ForumService.Application.Features.Forums.Commands.LeaveForum;
using ForumService.ForumService.Application.Features.Forums.Commands.RemoveMember;
using ForumService.ForumService.Application.Features.Forums.Commands.SetMemberPermissions;
using ForumService.ForumService.Application.Features.Forums.Commands.SetMemberRole;
using ForumService.ForumService.Application.Features.Forums.Queries.GetForumByName;
using ForumService.ForumService.Application.Features.Forums.Queries.GetForumMembers;
using ForumService.ForumService.Application.Features.Forums.Queries.GetForums;
using ForumService.ForumService.Application.Features.Forums.Queries.GetForumTags;
using ForumService.ForumService.Application.Features.Forums.Queries.GetJoinedForums;
using ForumService.ForumService.Application.Features.Forums.Queries.GetMemberPermissions;
using ForumService.ForumService.Application.Features.Forums.Queries.GetRecentForums;
using ForumService.ForumService.Application.Features.Forums.Queries.GetUserPermission;
using ForumService.ForumService.Application.Features.Forums.Queries.SearchForums;
using ForumService.ForumService.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumService.ForumService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<ForumDto>>> GetAllForums(CancellationToken cancellationToken)
        {
            var forums = await _mediator.Send(new GetForumsQuery(), cancellationToken);
            return Ok(forums);
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<ForumDto>>> SearchForums(
            [FromQuery] string q = "", CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new List<ForumDto>());

            var forums = await _mediator.Send(new SearchForumsQuery(q), cancellationToken);
            return Ok(forums);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateForum([FromBody] ForumDto forumDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var createdForum = await _mediator.Send(new CreateForumCommand(forumDto, Guid.Parse(userId)));

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

            await _mediator.Send(new JoinForumCommand(forumGuidParsed, Guid.Parse(userId)));
            return Ok();
        }

        [Authorize]
        [HttpPost("leave/{forumId}")]
        public async Task<ActionResult> LeaveForum(string forumId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!Guid.TryParse(forumId, out var forumGuidParsed))
                return BadRequest("Invalid forum ID");

            try
            {
                await _mediator.Send(new LeaveForumCommand(forumGuidParsed, Guid.Parse(userId)));
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{forumName}")]
        public async Task<ActionResult<ForumDto>> GetForum(string forumName, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userId,  out var parsedUserId);
            
            if (string.IsNullOrEmpty(forumName))
                return BadRequest("Invalid forum name");

            ForumDto? forum = await _mediator.Send(
                new GetForumByNameQuery(parsedUserId, forumName),
                cancellationToken);
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

            var permissions = await _mediator.Send(
                new GetUserPermissionQuery(forumGuid, userGuid),
                cancellationToken);
            return Ok(permissions);
        }

        [AllowAnonymous]
        [HttpGet("{forumId:guid}/tags")]
        public async Task<ActionResult<List<ForumTagDto>>> GetForumTags(Guid forumId, CancellationToken cancellationToken)
        {
            try
            {
                var tags = await _mediator.Send(new GetForumTagsQuery(forumId), cancellationToken);
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
                var created = await _mediator.Send(new CreateForumTagCommand(forumId, request, userGuid));
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

            var forums = await _mediator.Send(new GetJoinedForumsQuery(Guid.Parse(userId)), cancellationToken);
            return forums;
        }

        [Authorize]
        [HttpGet("recent")]
        public async Task<ActionResult<List<ForumDto>>> GetRecentForums(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }
            
            var forums = await _mediator.Send(new GetRecentForumsQuery(parsedUserId), cancellationToken);
            return forums;
        }

        // --- Member management endpoints ---

        [Authorize]
        [HttpGet("{forumId}/members")]
        public async Task<ActionResult<List<ForumMemberDto>>> GetForumMembers(Guid forumId, CancellationToken cancellationToken)
        {
            var members = await _mediator.Send(new GetForumMembersQuery(forumId), cancellationToken);
            return Ok(members);
        }

        [Authorize]
        [HttpGet("{forumId}/members/{targetUserId}/permissions")]
        public async Task<ActionResult<MemberPermissionDto?>> GetMemberPermissions(Guid forumId, Guid targetUserId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid))
                return Unauthorized();

            var result = await _mediator.Send(
                new GetMemberPermissionsQuery(forumId, targetUserId, userGuid),
                cancellationToken);
            if (result.Forbidden)
                return Forbid();

            return Ok(result.Permissions);
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
                await _mediator.Send(new SetMemberRoleCommand(forumId, targetUserId, request.Role, userGuid));
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
                await _mediator.Send(new SetMemberPermissionsCommand(
                    forumId,
                    targetUserId,
                    request.Permissions,
                    userGuid));
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
                await _mediator.Send(new RemoveMemberCommand(forumId, targetUserId, userGuid));
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
