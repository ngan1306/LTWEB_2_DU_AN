using KissAppTussing.Contracts;

namespace KissAppTussing.Services;

internal interface IAuthService
{
    Task<RegistrationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}

internal interface IDatingService
{
    Task<IReadOnlyList<UserProfileResponse>> DiscoverAsync(Guid userId, int limit, CancellationToken cancellationToken);
    Task<UserProfileResponse?> GetProfileAsync(Guid profileId, CancellationToken cancellationToken);
    Task<UserProfileResponse?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<SwipeResult> SwipeAsync(Guid userId, CreateSwipeRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<MatchResponse>> GetMatchesAsync(Guid userId, CancellationToken cancellationToken);
}

internal enum RegistrationStatus
{
    Created,
    EmailTaken,
    Underage
}

internal sealed record RegistrationResult(RegistrationStatus Status, AuthResponse? Response);

internal enum SwipeStatus
{
    Created,
    TargetNotFound,
    SelfSwipe,
    AlreadySwiped
}

internal sealed record SwipeResult(SwipeStatus Status, SwipeResponse? Response);