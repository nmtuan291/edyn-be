using System.Security.Claims;
using ChatService.ChatService.Application.DTOs;
using ChatService.ChatService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.ChatService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatController: ControllerBase
{
    private readonly IChatManagementService _chatManagementService;

    public ChatController(IChatManagementService chatManagementService)
    {
        _chatManagementService = chatManagementService;
    }

    [Authorize]
    [HttpGet("conversation")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        
        var conversations = await _chatManagementService.GetConversationsByUserId(userId); 
        return Ok(conversations);
    }

    [Authorize]
    [HttpGet("conversation/{conversationId}/messages")]
    public async Task<IActionResult> GetMessage(string conversationId)
    {
        if (!Guid.TryParse(conversationId, out var conversationGuid))
            return BadRequest("Invalid conversation ID");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var messages = await _chatManagementService.GetMessagesByConversationIdAsync(conversationGuid, userId);
        if (messages == null)
            return Forbid();

        return Ok(messages);
    }

    [Authorize]
    [HttpPost("conversation/add")]
    public async Task<IActionResult> AddMessage(MessageDto messageDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _chatManagementService.AddMessage(messageDto, userId);
        if (!result)
            return BadRequest("Failed to send message");

        return Ok();
    }
}