namespace VlogForge.Domain.Exceptions;

/// <summary>
/// Base exception for domain-level errors.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Gets the error code for this exception.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the DomainException class.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="message">The error message.</param>
    protected DomainException(string errorCode, string message) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the DomainException class with an inner exception.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected DomainException(string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// Gets the entity type that was not found.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the entity identifier that was not found.
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity identifier.</param>
    public EntityNotFoundException(string entityType, object entityId)
        : base("ENTITY_NOT_FOUND", $"{entityType} with id '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    /// <summary>
    /// Gets the name of the violated rule.
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// Initializes a new instance of the BusinessRuleException class.
    /// </summary>
    /// <param name="ruleName">The name of the violated rule.</param>
    /// <param name="message">The error message.</param>
    public BusinessRuleException(string ruleName, string message)
        : base("BUSINESS_RULE_VIOLATION", message)
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when there's a concurrency conflict.
/// </summary>
public class ConcurrencyException : DomainException
{
    /// <summary>
    /// Gets the entity type that had the concurrency conflict.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the entity identifier that had the concurrency conflict.
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="entityId">The entity identifier.</param>
    public ConcurrencyException(string entityType, object entityId)
        : base("CONCURRENCY_CONFLICT",
            $"The {entityType} with id '{entityId}' was modified by another process. Please refresh and try again.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when a team is not found.
/// Story: ACF-007
/// </summary>
public class TeamNotFoundException : DomainException
{
    public Guid TeamId { get; }

    public TeamNotFoundException(Guid teamId)
        : base("TEAM_NOT_FOUND", "Team not found.")
    {
        TeamId = teamId;
    }
}

/// <summary>
/// Exception thrown when a team member is not found.
/// Story: ACF-007
/// </summary>
public class TeamMemberNotFoundException : DomainException
{
    public Guid TeamId { get; }
    public Guid UserId { get; }

    public TeamMemberNotFoundException(Guid teamId, Guid userId)
        : base("TEAM_MEMBER_NOT_FOUND", "Team member not found.")
    {
        TeamId = teamId;
        UserId = userId;
    }
}

/// <summary>
/// Exception thrown when a team invitation is not found or expired.
/// Story: ACF-007
/// </summary>
public class TeamInvitationNotFoundException : DomainException
{
    public TeamInvitationNotFoundException()
        : base("INVITATION_NOT_FOUND", "Invitation not found or has expired.")
    {
    }
}

/// <summary>
/// Exception thrown when a team name already exists.
/// Story: ACF-007
/// </summary>
public class TeamNameAlreadyExistsException : DomainException
{
    public TeamNameAlreadyExistsException()
        : base("TEAM_NAME_EXISTS", "A team with this name already exists.")
    {
    }
}

/// <summary>
/// Exception thrown when access to a team resource is denied.
/// Story: ACF-007
/// </summary>
public class TeamAccessDeniedException : DomainException
{
    public Guid TeamId { get; }
    public Guid UserId { get; }

    public TeamAccessDeniedException(Guid teamId, Guid userId)
        : base("TEAM_ACCESS_DENIED", "You do not have permission to perform this action.")
    {
        TeamId = teamId;
        UserId = userId;
    }

    public TeamAccessDeniedException(string message)
        : base("TEAM_ACCESS_DENIED", message)
    {
        TeamId = Guid.Empty;
        UserId = Guid.Empty;
    }
}

/// <summary>
/// Exception thrown when a task assignment is not found.
/// Story: ACF-008
/// </summary>
public class TaskAssignmentNotFoundException : DomainException
{
    public Guid TaskAssignmentId { get; }

    public TaskAssignmentNotFoundException(Guid taskAssignmentId)
        : base("TASK_NOT_FOUND", "Task assignment not found.")
    {
        TaskAssignmentId = taskAssignmentId;
    }
}

/// <summary>
/// Exception thrown when a user doesn't have permission to modify a task.
/// Story: ACF-008
/// </summary>
public class TaskAccessDeniedException : DomainException
{
    public Guid TaskAssignmentId { get; }
    public Guid UserId { get; }

    public TaskAccessDeniedException(Guid taskAssignmentId, Guid userId)
        : base("TASK_ACCESS_DENIED", "You do not have permission to modify this task.")
    {
        TaskAssignmentId = taskAssignmentId;
        UserId = userId;
    }
}

/// <summary>
/// Exception thrown when a content item is not found.
/// Story: ACF-008
/// </summary>
public class ContentItemNotFoundException : DomainException
{
    public Guid ContentItemId { get; }

    public ContentItemNotFoundException(Guid contentItemId)
        : base("CONTENT_ITEM_NOT_FOUND", "Content item not found.")
    {
        ContentItemId = contentItemId;
    }
}

/// <summary>
/// Exception thrown when a collaboration request is not found.
/// Story: ACF-011
/// </summary>
public class CollaborationRequestNotFoundException : DomainException
{
    public Guid RequestId { get; }

    public CollaborationRequestNotFoundException(Guid requestId)
        : base("COLLABORATION_REQUEST_NOT_FOUND", "Collaboration request not found.")
    {
        RequestId = requestId;
    }
}

/// <summary>
/// Exception thrown when a user exceeds the daily collaboration request limit.
/// Story: ACF-011
/// </summary>
public class CollaborationRateLimitExceededException : DomainException
{
    public Guid UserId { get; }

    public CollaborationRateLimitExceededException(Guid userId)
        : base("COLLABORATION_RATE_LIMIT", "You have reached the maximum number of collaboration requests per day (5).")
    {
        UserId = userId;
    }
}

/// <summary>
/// Exception thrown when a duplicate pending collaboration request exists.
/// Story: ACF-011
/// </summary>
public class DuplicateCollaborationRequestException : DomainException
{
    public DuplicateCollaborationRequestException()
        : base("DUPLICATE_COLLABORATION_REQUEST", "A pending collaboration request already exists for this creator.")
    {
    }
}

/// <summary>
/// Exception thrown when the recipient is not open to collaborations.
/// Story: ACF-011
/// </summary>
public class RecipientNotOpenToCollaborationsException : DomainException
{
    public Guid RecipientId { get; }

    public RecipientNotOpenToCollaborationsException(Guid recipientId)
        : base("RECIPIENT_NOT_OPEN", "This creator is not currently open to collaborations.")
    {
        RecipientId = recipientId;
    }
}

/// <summary>
/// Exception thrown when a conversation is not found.
/// Story: ACF-012
/// </summary>
public class ConversationNotFoundException : DomainException
{
    public Guid ConversationId { get; }

    public ConversationNotFoundException(Guid conversationId)
        : base("CONVERSATION_NOT_FOUND", "Conversation not found.")
    {
        ConversationId = conversationId;
    }
}

/// <summary>
/// Exception thrown when a message is not found.
/// Story: ACF-012
/// </summary>
public class MessageNotFoundException : DomainException
{
    public Guid MessageId { get; }

    public MessageNotFoundException(Guid messageId)
        : base("MESSAGE_NOT_FOUND", "Message not found.")
    {
        MessageId = messageId;
    }
}

/// <summary>
/// Exception thrown when a user is not a participant in a conversation.
/// Story: ACF-012
/// </summary>
public class NotConversationParticipantException : DomainException
{
    public Guid ConversationId { get; }
    public Guid UserId { get; }

    public NotConversationParticipantException(Guid conversationId, Guid userId)
        : base("NOT_CONVERSATION_PARTICIPANT", "You are not a participant in this conversation.")
    {
        ConversationId = conversationId;
        UserId = userId;
    }
}

/// <summary>
/// Exception thrown when a user exceeds the messaging rate limit.
/// Story: ACF-012
/// </summary>
public class MessagingRateLimitExceededException : DomainException
{
    public Guid UserId { get; }

    public MessagingRateLimitExceededException(Guid userId)
        : base("MESSAGING_RATE_LIMIT", "You have reached the maximum number of messages per minute (60).")
    {
        UserId = userId;
    }
}

/// <summary>
/// Exception thrown when users don't have mutual collaboration to start a conversation.
/// Story: ACF-012
/// </summary>
public class NoMutualCollaborationException : DomainException
{
    public Guid UserId { get; }
    public Guid ParticipantId { get; }

    public NoMutualCollaborationException(Guid userId, Guid participantId)
        : base("NO_MUTUAL_COLLABORATION",
            "You can only message creators with an accepted collaboration request or shared team membership.")
    {
        UserId = userId;
        ParticipantId = participantId;
    }
}
