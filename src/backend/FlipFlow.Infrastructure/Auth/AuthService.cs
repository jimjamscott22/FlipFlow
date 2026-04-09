using FlipFlow.Application.Abstractions.Auth;
using FlipFlow.Application.Contracts.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlipFlow.Infrastructure.Auth;

public sealed class AuthService(
    UserManager<AppUser> userManager,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NormalizedEmail == request.Email.ToUpperInvariant(), cancellationToken);

        if (existingUser is not null)
        {
            throw new ValidationException("A user with this email already exists.");
        }

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.Select(error =>
                new FluentValidation.Results.ValidationFailure("identity", error.Description)));
        }

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.NormalizedEmail == request.Email.ToUpperInvariant(), cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return CreateAuthResponse(user);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User was not found.");

        return new CurrentUserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty
        };
    }

    private AuthResponseDto CreateAuthResponse(AppUser user)
    {
        var token = jwtTokenService.CreateToken(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName);

        return new AuthResponseDto
        {
            User = new CurrentUserDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty
            },
            Token = token
        };
    }
}
