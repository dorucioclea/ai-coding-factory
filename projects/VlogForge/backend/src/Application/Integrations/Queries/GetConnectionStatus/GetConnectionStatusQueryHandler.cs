using MediatR;
using VlogForge.Application.Common.Interfaces;
using VlogForge.Application.Integrations.DTOs;
using VlogForge.Domain.Entities;

namespace VlogForge.Application.Integrations.Queries.GetConnectionStatus;

/// <summary>
/// Handler for GetConnectionStatusQuery.
/// Story: ACF-003
/// </summary>
public sealed class GetConnectionStatusQueryHandler
    : IRequestHandler<GetConnectionStatusQuery, ConnectionStatusResponse>
{
    private readonly IPlatformConnectionRepository _connectionRepository;

    // Platforms supported for OAuth integration
    private static readonly string[] SupportedPlatforms = new[]
    {
        PlatformType.YouTube.ToString(),
        PlatformType.Instagram.ToString(),
        PlatformType.TikTok.ToString()
    };

    public GetConnectionStatusQueryHandler(IPlatformConnectionRepository connectionRepository)
    {
        _connectionRepository = connectionRepository;
    }

    public async Task<ConnectionStatusResponse> Handle(
        GetConnectionStatusQuery request,
        CancellationToken cancellationToken)
    {
        var connections = await _connectionRepository.GetByUserIdAsync(
            request.UserId,
            cancellationToken);

        var connectionDtos = connections.Select(c => c.ToDto()).ToList();

        // Determine which platforms are available (not yet connected)
        var connectedPlatforms = connections
            .Where(c => c.Status != ConnectionStatus.Disconnected)
            .Select(c => c.PlatformType.ToString())
            .ToHashSet();

        var availablePlatforms = SupportedPlatforms
            .Where(p => !connectedPlatforms.Contains(p))
            .ToList();

        return new ConnectionStatusResponse(connectionDtos, availablePlatforms);
    }
}
