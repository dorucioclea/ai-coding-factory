using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VlogForge.Api.Controllers.Conversations.Requests;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.Commands.MarkMessagesAsRead;
using VlogForge.Application.Messaging.Commands.SendMessage;
using VlogForge.Application.Messaging.Commands.StartConversation;
using VlogForge.Application.Messaging.DTOs;
using VlogForge.Application.Messaging.Queries.GetConversations;
using VlogForge.Application.Messaging.Queries.GetMessages;
using VlogForge.Application.Messaging.Queries.GetUnreadCount;
using VlogForge.Domain.Exceptions;

namespace VlogForge.Api.Controllers.Conversations;

/// <summary>
/// Conversation and messaging endpoints.
/// Story: ACF-012
/// </summary>
[ApiController]
[Route("api/conversations")]
[Authorize]
[Produces("application/json")]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ConversationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Starts a new conversation or returns an existing one.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConversationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationResponse>> StartConversation(
        [FromBody] StartConversationRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new StartConversationCommand(userId, request.ParticipantId);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetConversations), null, result);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (NoMutualCollaborationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the user's conversations.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ConversationListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ConversationListResponse>> GetConversations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = new GetConversationsQuery(userId, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets messages in a conversation.
    /// </summary>
    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(MessageListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageListResponse>> GetMessages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = new GetMessagesQuery(userId, id, page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (ConversationNotFoundException)
        {
            return NotFound(new { error = "Conversation not found." });
        }
        catch (NotConversationParticipantException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Sends a message in a conversation.
    /// </summary>
    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<MessageResponse>> SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new SendMessageCommand(userId, id, request.Content);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetMessages), new { id }, result);
        }
        catch (ConversationNotFoundException)
        {
            return NotFound(new { error = "Conversation not found." });
        }
        catch (NotConversationParticipantException)
        {
            return Forbid();
        }
        catch (MessagingRateLimitExceededException ex)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Marks all unread messages in a conversation as read.
    /// </summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var command = new MarkMessagesAsReadCommand(userId, id);
            var count = await _mediator.Send(command);
            return Ok(new { markedAsRead = count });
        }
        catch (ConversationNotFoundException)
        {
            return NotFound(new { error = "Conversation not found." });
        }
        catch (NotConversationParticipantException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Gets the total unread message count.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var query = new GetUnreadCountQuery(userId);
        var count = await _mediator.Send(query);
        return Ok(new { unreadCount = count });
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException("User ID not found in claims.");
        return userId;
    }
}
