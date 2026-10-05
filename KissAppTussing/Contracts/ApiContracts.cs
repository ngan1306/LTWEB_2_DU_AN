using System.ComponentModel.DataAnnotations;

namespace KissAppTussing.Contracts;

/// <summary>Payload for creating a dating app account.</summary>
public sealed record RegisterRequest
{
    /// <summary>The display name shown on the user's profile.</summary>
    [Required, StringLength(80, MinimumLength = 2)]
    public required string Name { get; init; }

    /// <summary>The account email address.</summary>
    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; init; }

    /// <summary>A password containing at least eight characters.</summary>
    [Required, MinLength(8), StringLength(128)]
    public required string Password { get; init; }

    /// <summary>The user's date of birth.</summary>
    public required DateOnly BirthDate { get; init; }

    /// <summary>A short introduction shown on the profile.</summary>
    [StringLength(500)]
    public string Bio { get; init; } = string.Empty;
}

/// <summary>Payload for signing in to an existing account.</summary>
public sealed record LoginRequest
{
    /// <summary>The account email address.</summary>
    [Required, EmailAddress, StringLength(254)]
    public required string Email { get; init; }

    /// <summary>The account password.</summary>
    [Required, StringLength(128)]
    public required string Password { get; init; }
}

/// <summary>Payload for updating the signed-in user's profile.</summary>
public sealed record UpdateProfileRequest
{
    /// <summary>The display name shown on the profile.</summary>
    [Required, StringLength(80, MinimumLength = 2)]
    public required string Name { get; init; }

    /// <summary>The user's date of birth.</summary>
    public required DateOnly BirthDate { get; init; }

    /// <summary>A short introduction shown on the profile.</summary>
    [StringLength(500)]
    public string Bio { get; init; } = string.Empty;
}

/// <summary>Payload for liking or passing on another profile.</summary>
public sealed record CreateSwipeRequest
{
    /// <summary>The profile being viewed.</summary>
    public required Guid TargetUserId { get; init; }

    /// <summary>True to like the profile; false to pass.</summary>
    public bool IsLike { get; init; }
}

/// <summary>Public profile information visible to other users.</summary>
/// <param name="Id">The profile identifier.</param>
/// <param name="Name">The display name.</param>
/// <param name="BirthDate">The user's date of birth.</param>
/// <param name="Bio">The user's introduction.</param>
/// <param name="CreatedAt">When the account was created.</param>
public sealed record UserProfileResponse(
    Guid Id,
    string Name,
    DateOnly BirthDate,
    string Bio,
    DateTimeOffset CreatedAt);

/// <summary>Authentication token and the signed-in user's profile.</summary>
/// <param name="AccessToken">The bearer token used for authenticated requests.</param>
/// <param name="TokenType">The token type, always Bearer.</param>
/// <param name="ExpiresAt">When the token expires.</param>
/// <param name="Profile">The signed-in user's public profile.</param>
public sealed record AuthResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    UserProfileResponse Profile);

/// <summary>Result of liking or passing on a profile.</summary>
/// <param name="TargetUserId">The profile that was viewed.</param>
/// <param name="IsLike">Whether the profile was liked.</param>
/// <param name="IsMatch">Whether this action created a mutual match.</param>
/// <param name="MatchId">The new match identifier, when a match was created.</param>
public sealed record SwipeResponse(
    Guid TargetUserId,
    bool IsLike,
    bool IsMatch,
    Guid? MatchId);

/// <summary>A match and the other user's profile.</summary>
/// <param name="Id">The match identifier.</param>
/// <param name="Profile">The other matched user's profile.</param>
/// <param name="MatchedAt">When the mutual match was created.</param>
public sealed record MatchResponse(
    Guid Id,
    UserProfileResponse Profile,
    DateTimeOffset MatchedAt);