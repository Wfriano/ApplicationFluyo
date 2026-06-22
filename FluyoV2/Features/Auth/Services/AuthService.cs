using FluyoV2.Exceptions;
using FluyoV2.Features.Auth.Dtos;
using FluyoV2.Features.Auth.Models;
using FluyoV2.Features.Auth.Repositories;
using FluyoV2.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FluyoV2.Features.Auth.Services;

public class AuthService
{
    private readonly AuthRepository _authRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AuthRepository authRepository,
        JwtSettings jwtSettings,
        ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var existingUser =
            await _authRepository.GetByEmailAsync(email);

        if (existingUser is not null)
        {
            _logger.LogWarning(
                "Intento de registro con correo existente: {Email}",
                email);

            throw new BusinessException(
                "Ya existe un usuario registrado con este correo.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password),
            EmailVerified = false,
            RefreshToken = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        await _authRepository.CreateAsync(user);

        _logger.LogInformation(
            "Usuario registrado correctamente: {Email}",
            user.Email);

        var token = GenerateJwtToken(user);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = user.RefreshToken,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user =
            await _authRepository.GetByEmailAsync(email);

        if (user is null)
        {
            _logger.LogWarning(
                "Intento de login fallido. Usuario no existe: {Email}",
                email);

            throw new BusinessException(
                "Correo o contraseña incorrectos.");
        }

        var validPassword =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!validPassword)
        {
            _logger.LogWarning(
                "Intento de login fallido. Contraseña inválida: {Email}",
                email);

            throw new BusinessException(
                "Correo o contraseña incorrectos.");
        }

        var refreshToken =
            Guid.NewGuid().ToString();

        await _authRepository
            .UpdateRefreshTokenAsync(
                user.Id,
                refreshToken);

        user.RefreshToken = refreshToken;

        var token = GenerateJwtToken(user);

        _logger.LogInformation(
            "Login exitoso para usuario: {Email}",
            user.Email);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                ClaimTypes.Email,
                user.Email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _jwtSettings.Key));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _jwtSettings.ExpiresInMinutes),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}