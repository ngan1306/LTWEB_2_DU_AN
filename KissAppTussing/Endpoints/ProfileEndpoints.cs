using KissAppTussing.Contracts;
using KissAppTussing.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace KissAppTussing.Endpoints;

internal static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var profiles = endpoints.MapGroup("/api/profiles")
            .RequireAuthorization();

        profiles.MapGet("/discover", DiscoverAsync)
            .WithName("DiscoverProfiles")
            .WithSummary("Discover profiles that have not been swiped")
            .WithDescription("Returns up to 100 profiles, excluding the signed-in user and profiles already swiped.")
            .ProducesValidationProblem();

        profiles.MapGet("/me", GetMyProfileAsync)
            .WithName("GetMyProfile")
            .WithSummary("Get the signed-in user's profile");

        profiles.MapPut("/me", UpdateMyProfileAsync)
            .WithName("UpdateMyProfile")
            .WithSummary("Update the signed-in user's profile")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        profiles.MapGet("/{profileId:guid}", GetProfileAsync)
            .WithName("GetProfile")
            .WithSummary("Get a public dating profile");

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<UserProfileResponse>>> DiscoverAsync(
        int? limit,
        System.Security.Claims.ClaimsPrincipal principal,
        IDatingService datingService,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(limit ?? 20, 1, 100);
        return TypedResults.Ok(await datingService.DiscoverAsync(
            CurrentUser.GetId(principal), pageSize, cancellationToken));
    }

    private static async Task<Results<Ok<UserProfileResponse>, NotFound>> GetMyProfileAsync(
        System.Security.Claims.ClaimsPrincipal principal,
        IDatingService datingService,
        CancellationToken cancellationToken)
    {
        var profile = await datingService.GetProfileAsync(CurrentUser.GetId(principal), cancellationToken);
        return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
    }

    private static async Task<Results<Ok<UserProfileResponse>, NotFound, BadRequest<ProblemDetails>>> UpdateMyProfileAsync(
        UpdateProfileRequest request,
        System.Security.Claims.ClaimsPrincipal principal,
        IDatingService datingService,
        CancellationToken cancellationToken)
    {
        var profile = await datingService.UpdateProfileAsync(
            CurrentUser.GetId(principal), request, cancellationToken);

        if (profile is not null)
        {
            return TypedResults.Ok(profile);
        }

        var existingProfile = await datingService.GetProfileAsync(CurrentUser.GetId(principal), cancellationToken);
        return existingProfile is null
            ? TypedResults.NotFound()
            : TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Age requirement not met",
                Status = StatusCodes.Status400BadRequest,
                Detail = "A profile must belong to a user who is at least 18 years old."
            });
    }

    private static async Task<Results<Ok<UserProfileResponse>, NotFound>> GetProfileAsync(
        Guid profileId,
        IDatingService datingService,
        CancellationToken cancellationToken)
    {
        var profile = await datingService.GetProfileAsync(profileId, cancellationToken);
        return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
    }
}