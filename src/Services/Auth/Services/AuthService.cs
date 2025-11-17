using Apivia.Services.Auth.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Apivia.Services.Auth.Services;

/// <summary>
/// Service for authentication operations
/// </summary>
public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<EmailConfirmationTokenResponse> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ConfirmEmailResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of authentication service
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ApiviaDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IDistributedCache _cache;

    public AuthService(
        UserManager<User> userManager,
        ApiviaDbContext context,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IDistributedCache cache)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            EmailConfirmed = false, // Email confirmation required for security
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User registration failed: {Errors}", errors);
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        _logger.LogInformation("User {UserId} registered successfully", user.Id);

        return new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Message = "Registration successful"
        };
    }

    /// <summary>
    /// Login user and generate tokens
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt for non-existent user: {Email}", request.Email);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Account is inactive");
        }

        // Verify password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Invalid password attempt for user: {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Store refresh token in Redis with 7 days expiration
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        var tokenData = new RefreshTokenData
        {
            UserId = user.Id,
            ExpiresAt = refreshTokenExpiry
        };

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = refreshTokenExpiry
        };

        await _cache.SetStringAsync(
            $"refresh_token:{refreshToken}",
            JsonSerializer.Serialize(tokenData),
            options,
            cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        var expirationMinutes = int.Parse(_userManager.Options.Tokens.AuthenticatorTokenProvider ?? "60");

        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Default 60 minutes
        };
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        // Validate refresh token from Redis
        var cachedData = await _cache.GetStringAsync($"refresh_token:{request.RefreshToken}", cancellationToken);
        if (string.IsNullOrEmpty(cachedData))
        {
            _logger.LogWarning("Invalid refresh token attempt");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        RefreshTokenData? tokenData;
        try
        {
            tokenData = JsonSerializer.Deserialize<RefreshTokenData>(cachedData);
        }
        catch (JsonException)
        {
            _logger.LogError("Failed to deserialize refresh token data");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (tokenData == null)
        {
            _logger.LogError("Refresh token data is null");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Check if token is expired (Redis should auto-expire, but double-check)
        if (tokenData.ExpiresAt < DateTime.UtcNow)
        {
            await _cache.RemoveAsync($"refresh_token:{request.RefreshToken}", cancellationToken);
            _logger.LogWarning("Expired refresh token for user: {UserId}", tokenData.UserId);
            throw new UnauthorizedAccessException("Refresh token expired");
        }

        // Find user
        var user = await _userManager.FindByIdAsync(tokenData.UserId.ToString());
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Refresh token for non-existent or inactive user: {UserId}", tokenData.UserId);
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        // Generate new tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // Remove old refresh token from Redis
        await _cache.RemoveAsync($"refresh_token:{request.RefreshToken}", cancellationToken);

        // Store new refresh token in Redis
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        var newTokenData = new RefreshTokenData
        {
            UserId = user.Id,
            ExpiresAt = newRefreshTokenExpiry
        };

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = newRefreshTokenExpiry
        };

        await _cache.SetStringAsync(
            $"refresh_token:{newRefreshToken}",
            JsonSerializer.Serialize(newTokenData),
            options,
            cancellationToken);

        _logger.LogInformation("Token refreshed for user {UserId}", user.Id);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    /// <summary>
    /// Change user password
    /// </summary>
    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, errors);
            throw new InvalidOperationException($"Password change failed: {errors}");
        }

        _logger.LogInformation("Password changed successfully for user {UserId}", userId);

        return true;
    }

    /// <summary>
    /// Generate email confirmation token for a user
    /// </summary>
    public async Task<EmailConfirmationTokenResponse> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.EmailConfirmed)
        {
            throw new InvalidOperationException("Email is already confirmed");
        }

        // Generate email confirmation token using UserManager
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // In production, send this token via email
        _logger.LogInformation("Email confirmation token generated for user {UserId}. Token: {Token}", userId, token);

        return new EmailConfirmationTokenResponse
        {
            UserId = userId,
            Token = token,
            Message = "Email confirmation token generated. In production, this would be sent via email."
        };
    }

    /// <summary>
    /// Confirm user email with token
    /// </summary>
    public async Task<ConfirmEmailResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.EmailConfirmed)
        {
            return new ConfirmEmailResponse
            {
                Success = true,
                Message = "Email is already confirmed"
            };
        }

        // Confirm email using UserManager
        var result = await _userManager.ConfirmEmailAsync(user, request.Token);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}", request.UserId, errors);
            throw new InvalidOperationException($"Email confirmation failed: {errors}");
        }

        _logger.LogInformation("Email confirmed successfully for user {UserId}", request.UserId);

        return new ConfirmEmailResponse
        {
            Success = true,
            Message = "Email confirmed successfully"
        };
    }
}

/// <summary>
/// Internal class for refresh token data storage in Redis
/// </summary>
internal class RefreshTokenData
{
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
