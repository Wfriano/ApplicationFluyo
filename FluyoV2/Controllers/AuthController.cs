using FluentValidation;
using FluyoV2.Controllers.Base;
using FluyoV2.Features.Auth.Dtos;
using FluyoV2.Features.Auth.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace FluyoV2.Controllers;

[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly AuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly FluyoV2.Users.Repositories.IUserRepository _userRepository;
    private readonly FluyoV2.Services.IEmailService _emailService;

    public AuthController(
        AuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        FluyoV2.Users.Repositories.IUserRepository userRepository,
        FluyoV2.Services.IEmailService emailService)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var response = await _authService.RegisterAsync(request);

        return Success(
            response,
            "Usuario registrado correctamente");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return Failure(validation.Errors.First().ErrorMessage);

        var response = await _authService.LoginAsync(request);

        return Success(
            response,
            "Inicio de sesión exitoso");
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        var result = new
        {
            Application = "Fluyo V2",
            Status = "Running"
        };

        return Success(
            result,
            "API funcionando correctamente");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
            return Failure("El correo es obligatorio");

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
            return Failure("Correo no existe");

        // generate temporary password
        var tempPassword = GenerateTemporaryPassword();

        // hash and update
        var newHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        await _userRepository.UpdatePasswordAsync(user.Id, newHash, DateTime.UtcNow);

        // compose email template
        var subject = "Recuperación de contraseña Fluyo";
        var body = $@"<div style='font-family: Arial, sans-serif; max-width:600px; margin:0 auto; padding:20px; border:1px solid #eee; border-radius:8px;'>
            <h2 style='color:#333;'>Hola {System.Net.WebUtility.HtmlEncode(user.FullName ?? "")},</h2>
            <p style='color:#555;'>Has solicitado recuperar tu contraseña. Tu nueva contraseña temporal es:</p>
            <p style='font-size:18px; font-weight:600; color:#1a73e8;'>{System.Net.WebUtility.HtmlEncode(tempPassword)}</p>
            <p style='color:#555;'>Te recomendamos cambiarla después de iniciar sesión.</p>
            <hr />
            <p style='color:#999; font-size:12px;'>Si no solicitaste este cambio, por favor contacta al soporte.</p>
        </div>";

        await _emailService.SendEmailAsync(user.Email, subject, body);

        return Success<object>(null, "Correo enviado con la contraseña temporal");
    }

    private static string GenerateTemporaryPassword(int length = 10)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rnd = new System.Security.Cryptography.RNGCryptoServiceProvider();
        var data = new byte[length];
        rnd.GetBytes(data);
        var result = new char[length];
        for (int i = 0; i < length; i++)
        {
            var idx = data[i] % chars.Length;
            result[i] = chars[idx];
        }
        return new string(result);
    }
}