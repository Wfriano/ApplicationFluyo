using System.Text.RegularExpressions;
using FluyoV2.Users.Dtos;
using FluyoV2.Users.Repositories;

namespace FluyoV2.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Error("No fue posible identificar al usuario autenticado.");
        }

        if (request is null)
        {
            return Error("La información enviada no es válida.");
        }

        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            return Error("El usuario no fue encontrado.");
        }

        var currentPasswordIsValid = BCrypt.Net.BCrypt.Verify(
            request.CurrentPassword,
            user.PasswordHash
        );

        if (!currentPasswordIsValid)
        {
            return Error("La contraseña actual es incorrecta.");
        }

        var isSamePassword = BCrypt.Net.BCrypt.Verify(
            request.NewPassword,
            user.PasswordHash
        );

        if (isSamePassword)
        {
            return Error(
                "La nueva contraseña debe ser diferente a la contraseña actual."
            );
        }

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(
            request.NewPassword,
            workFactor: 11
        );

        var passwordUpdatedAt = DateTime.UtcNow;

        var updated = await _userRepository.UpdatePasswordAsync(
            userId,
            newPasswordHash,
            passwordUpdatedAt
        );

        if (!updated)
        {
            return Error(
                "No fue posible actualizar la contraseña. Inténtalo nuevamente."
            );
        }

        return new ChangePasswordResponse
        {
            Success = true,
            Message = "Contraseña actualizada correctamente."
        };
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

    private static ChangePasswordResponse Error(string message)
    {
        return new ChangePasswordResponse
        {
            Success = false,
            Message = message
        };
    }
}