using System.Net.Mail;
using UserManagementAPI.Models;
using UserManagementAPI.Interfaces;

namespace UserManagementAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private const int MAX_AGE = 150;
    private const int MAX_USERNAME_LENGTH = 100;
    private const int MIN_PASSWORD_LENGTH = 6;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return _repository.GetAllUsersAsync();
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        return _repository.GetUserByIdAsync(id);
    }

    // Keep these as tuples - they have business logic and validation
    public async Task<(bool success, string? errorMessage, User? user)> CreateUserAsync(User? user)
    {
        var validation = ValidateUser(user);
        if (!validation.isValid)
        {
            return (false, validation.errorMessage, null);
        }

        var createdUser = await _repository.CreateUserAsync(user!);
        return (true, null, createdUser);
    }

    public async Task<(bool success, string? errorMessage, User? user)> UpdateUserAsync(int id, User? user)
    {
        var validation = ValidateUser(user);
        if (!validation.isValid)
        {
            return (false, validation.errorMessage, null);
        }

        var updatedUser = await _repository.UpdateUserAsync(id, user!);
        if (updatedUser == null)
        {
            return (false, $"User with ID {id} not found", null);
        }
        
        return (true, null, updatedUser);
    }

    public async Task<(bool success, string? errorMessage)> DeleteUserAsync(int id)
    {
        bool deleted = await _repository.DeleteUserAsync(id);
        if (deleted)
        {
            return (true, null);
        }
        return (false, $"User with ID {id} not found");
    }

    public Task<bool> UserExistsAsync(int id)
    {
        return _repository.UserExistsAsync(id);
    }

    private static (bool isValid, string? errorMessage) ValidateUser(User? user)
    {
        if (user == null)
            return (false, "User data is required");
        
        if (string.IsNullOrWhiteSpace(user.Username))
            return (false, "Username is required and cannot be empty");
        
        if (user.Username.Length > MAX_USERNAME_LENGTH)
            return (false, $"Username cannot exceed {MAX_USERNAME_LENGTH} characters");
        
        if (string.IsNullOrWhiteSpace(user.Email))
            return (false, "Email is required and cannot be empty");
        
        if (!IsValidEmail(user.Email))
            return (false, "Email format is invalid");
        
        if (user.Age < 0 || user.Age > MAX_AGE)
            return (false, $"Age must be between 0 and {MAX_AGE}");
        
        if (string.IsNullOrWhiteSpace(user.Password))
            return (false, "Password is required and cannot be empty");
        
        if (user.Password.Length < MIN_PASSWORD_LENGTH)
            return (false, $"Password must be at least {MIN_PASSWORD_LENGTH} characters long");
        
        return (true, null);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}