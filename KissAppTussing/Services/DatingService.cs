using KissAppTussing.Contracts;
using KissAppTussing.Data;
using KissAppTussing.Models;
using Microsoft.EntityFrameworkCore;

namespace KissAppTussing.Services;

internal sealed class DatingService(DatingDbContext database) : IDatingService
{
    public async Task<IReadOnlyList<UserProfileResponse>> DiscoverAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken)
    {
        var users = await database.Users
            .AsNoTracking()
            .Where(user => user.Id != userId)
            .Where(user => !database.Swipes.Any(swipe =>
                swipe.FromUserId == userId && swipe.ToUserId == user.Id))
            .OrderBy(user => user.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return users.Select(ToProfile).ToArray();
    }

    public async Task<UserProfileResponse?> GetProfileAsync(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        var user = await database.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == profileId, cancellationToken);

        return user is null ? null : ToProfile(user);
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await database.Users
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.BirthDate.Year;
        if (request.BirthDate > today.AddYears(-age))
        {
            age--;
        }

        if (age < 18)
        {
            return null;
        }

        user.Name = request.Name.Trim();
        user.BirthDate = request.BirthDate;
        user.Bio = request.Bio.Trim();
        await database.SaveChangesAsync(cancellationToken);

        return ToProfile(user);
    }

    public async Task<SwipeResult> SwipeAsync(
        Guid userId,
        CreateSwipeRequest request,
        CancellationToken cancellationToken)
    {
        if (userId == request.TargetUserId)
        {
            return new SwipeResult(SwipeStatus.SelfSwipe, null);
        }

        var targetExists = await database.Users
            .AnyAsync(user => user.Id == request.TargetUserId, cancellationToken);
        if (!targetExists)
        {
            return new SwipeResult(SwipeStatus.TargetNotFound, null);
        }

        var alreadySwiped = await database.Swipes.AnyAsync(
            swipe => swipe.FromUserId == userId && swipe.ToUserId == request.TargetUserId,
            cancellationToken);
        if (alreadySwiped)
        {
            return new SwipeResult(SwipeStatus.AlreadySwiped, null);
        }

        database.Swipes.Add(new Swipe
        {
            FromUserId = userId,
            ToUserId = request.TargetUserId,
            IsLike = request.IsLike
        });

        Guid? matchId = null;
        if (request.IsLike)
        {
            var mutualLike = await database.Swipes.AnyAsync(
                swipe => swipe.FromUserId == request.TargetUserId
                    && swipe.ToUserId == userId
                    && swipe.IsLike,
                cancellationToken);

            if (mutualLike)
            {
                var firstUserId = userId.CompareTo(request.TargetUserId) < 0 ? userId : request.TargetUserId;
                var secondUserId = userId.CompareTo(request.TargetUserId) < 0 ? request.TargetUserId : userId;
                var match = new DatingMatch
                {
                    FirstUserId = firstUserId,
                    SecondUserId = secondUserId
                };
                database.Matches.Add(match);
                matchId = match.Id;
            }
        }

        await database.SaveChangesAsync(cancellationToken);

        return new SwipeResult(
            SwipeStatus.Created,
            new SwipeResponse(request.TargetUserId, request.IsLike, matchId is not null, matchId));
    }

    public async Task<IReadOnlyList<MatchResponse>> GetMatchesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var matches = await database.Matches
            .AsNoTracking()
            .Where(match => match.FirstUserId == userId || match.SecondUserId == userId)
            .Include(match => match.FirstUser)
            .Include(match => match.SecondUser)
            .OrderByDescending(match => match.MatchedAt)
            .ToListAsync(cancellationToken);

        return matches.Select(match => new MatchResponse(
            match.Id,
            ToProfile(match.FirstUserId == userId ? match.SecondUser : match.FirstUser),
            match.MatchedAt)).ToArray();
    }

    private static UserProfileResponse ToProfile(User user) => new(
        user.Id,
        user.Name,
        user.BirthDate,
        user.Bio,
        user.CreatedAt);
}