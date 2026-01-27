using MediatR;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Queries.GetProfileByUsername;

/// <summary>
/// Query to get a public profile by username.
/// Story: ACF-002
/// </summary>
public sealed record GetProfileByUsernameQuery(string Username) : IRequest<PublicProfileResponse>;
