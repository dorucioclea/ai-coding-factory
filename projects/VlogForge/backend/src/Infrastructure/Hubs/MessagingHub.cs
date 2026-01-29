using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Messaging.DTOs;

namespace VlogForge.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time messaging.
/// Story: ACF-012
/// </summary>
[Authorize]
public sealed partial class MessagingHub : Hub
{
    private readonly IConversationRepository _conversationRepo;
    private readonly ILogger<MessagingHub> _logger;

    public MessagingHub(
        IConversationRepository conversationRepo,
        ILogger<MessagingHub> logger)
    {
        _conversationRepo = conversationRepo;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId is not null)
        {
            // Add user to personal group for notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            LogUserConnected(_logger, userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId is not null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
            LogUserDisconnected(_logger, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a conversation group to receive real-time messages.
    /// Verifies the caller is a participant before granting access.
    /// </summary>
    public async Task JoinConversation(Guid conversationId)
    {
        if (!await VerifyParticipantAsync(conversationId))
            return;

        var groupName = $"conversation:{conversationId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        LogJoinedConversation(_logger, GetUserId() ?? "unknown", conversationId);
    }

    /// <summary>
    /// Leave a conversation group.
    /// </summary>
    public async Task LeaveConversation(Guid conversationId)
    {
        var groupName = $"conversation:{conversationId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Broadcasts a typing indicator to the conversation group.
    /// Verifies the caller is a participant.
    /// </summary>
    public async Task SendTypingIndicator(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId is null) return;

        if (!await VerifyParticipantAsync(conversationId))
            return;

        await Clients.OthersInGroup($"conversation:{conversationId}")
            .SendAsync("UserTyping", conversationId, userId);
    }

    /// <summary>
    /// Stops the typing indicator broadcast.
    /// Verifies the caller is a participant.
    /// </summary>
    public async Task StopTypingIndicator(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId is null) return;

        if (!await VerifyParticipantAsync(conversationId))
            return;

        await Clients.OthersInGroup($"conversation:{conversationId}")
            .SendAsync("UserStoppedTyping", conversationId, userId);
    }

    private async Task<bool> VerifyParticipantAsync(Guid conversationId)
    {
        var userIdStr = GetUserId();
        if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
            return false;

        var conversation = await _conversationRepo.GetByIdAsync(conversationId);
        if (conversation is null || !conversation.IsParticipant(userId))
        {
            await Clients.Caller.SendAsync("Error", "Not authorized to join this conversation.");
            return false;
        }

        return true;
    }

    private string? GetUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} connected to messaging hub")]
    private static partial void LogUserConnected(ILogger logger, string userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} disconnected from messaging hub")]
    private static partial void LogUserDisconnected(ILogger logger, string userId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "User {UserId} joined conversation {ConversationId}")]
    private static partial void LogJoinedConversation(ILogger logger, string userId, Guid conversationId);
}
