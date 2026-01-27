using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Profiles.DTOs;

namespace VlogForge.Application.Profiles.Queries.GetMyProfile;

/// <summary>
/// Handler for GetMyProfileQuery.
/// Story: ACF-002
/// </summary>
public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, CreatorProfileResponse?>
{
    private readonly ICreatorProfileRepository _profileRepository;

    public GetMyProfileQueryHandler(ICreatorProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<CreatorProfileResponse?> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _profileRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return profile is null ? null : CreatorProfileResponse.FromEntity(profile);
    }
}
