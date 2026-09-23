using FluyoV2.Infrastructure;
using FluyoV2.Users.Dtos;
using FluyoV2.Users.Repositories;

namespace FluyoV2.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    public UserService(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Error("No fue posible identificar al usuario autenticado.");

        if (request is null)
            return Error("La información enviada no es válida.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Error("El usuario no fue encontrado.");

        var currentPasswordIsValid = BCrypt.Net.BCrypt.Verify(
            request.CurrentPassword,
            user.PasswordHash
        );

        if (!currentPasswordIsValid)
            return Error("La contraseña actual es incorrecta.");

        var isSamePassword = BCrypt.Net.BCrypt.Verify(
            request.NewPassword,
            user.PasswordHash
        );

        if (isSamePassword)
            return Error("La nueva contraseña debe ser diferente a la contraseña actual.");

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 11);
        var updated = await _userRepository.UpdatePasswordAsync(userId, newPasswordHash, DateTime.UtcNow);

        if (!updated)
            return Error("No fue posible actualizar la contraseña. Inténtalo nuevamente.");

        return new ChangePasswordResponse { Success = true, Message = "Contraseña actualizada correctamente." };
    }

    public async Task<UserResponse?> GetProfileAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        return new UserResponse
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DateOfBirth = user.DateOfBirth,
            PhotoUser = user.PhotoUser
        };
    }

    public async Task<UserResponse?> UpdateProfileAsync(string userId, UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId) || request is null)
            return null;

        var fullName = request.FullName.Trim();

        var updated = await _userRepository.UpdateProfileAsync(
            userId,
            fullName,
            request.Email,
            request.PhoneNumber?.Trim(),
            request.DateOfBirth,
            request.PhotoUser
        );

        if (!updated)
            return null;

        return await GetProfileAsync(userId);
    }

    public async Task<Result> SendPasswordByEmail(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
            return Result.Failure("Usuario no encontrado.");

        var subject = "Recuperación de contraseña - Fluyo";
        var body = $@"
        Hola {user.FullName},

        Gracias por comunicarte con nosotros para solicitar la recuperación tu contraseña en Fluyo.
        Tu contraseña actual es: {user.PasswordHash}

        Si no solicitaste este correo, ignóralo.

        Equipo Fluyo
        ";

        await _emailService.SendEmailAsync(email, subject, body);

        return Result.SuccessResult("Correo enviado con la contraseña.");
    }

    public async Task<Result> ChangePassword(string userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result.Failure("Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return Result.Failure("La contraseña actual es incorrecta.");

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 11);
        var updated = await _userRepository.UpdatePasswordAsync(userId, newPasswordHash, DateTime.UtcNow);

        if (!updated)
            return Result.Failure("No fue posible actualizar la contraseña.");

        return Result.SuccessResult("Contraseña cambiada correctamente.");
    }

    private static ChangePasswordResponse Error(string message) =>
        new ChangePasswordResponse { Success = false, Message = message };
}
