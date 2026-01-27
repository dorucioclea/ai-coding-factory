using MediatR;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Queries.GetMyProfile;

/// <summary>
/// Query to get the current user's profile.
/// Story: ACF-002
/// </summary>
public sealed record GetMyProfileQuery(Guid UserId) : IRequest<CreatorProfileResponse?>;
