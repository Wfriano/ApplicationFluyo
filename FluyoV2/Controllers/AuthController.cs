using FluentValidation;
using FluyoV2.Controllers.Base;
using FluyoV2.Features.Auth.Dtos;
using FluyoV2.Features.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace FluyoV2.Controllers;

[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly AuthService _authService;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        AuthService authService,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
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
}