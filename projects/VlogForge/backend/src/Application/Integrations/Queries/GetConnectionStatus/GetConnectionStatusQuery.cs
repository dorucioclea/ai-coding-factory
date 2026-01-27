using MediatR;
using VlogForge.Application.Integrations.DTOs;

namespace VlogForge.Application.Integrations.Queries.GetConnectionStatus;

/// <summary>
/// Query to get all platform connection statuses for a user.
/// Story: ACF-003
/// </summary>
public sealed record GetConnectionStatusQuery(Guid UserId) : IRequest<ConnectionStatusResponse>;
