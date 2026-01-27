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
