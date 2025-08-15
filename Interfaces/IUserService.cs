using UserManagementAPI.Models;

namespace UserManagementAPI.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<(bool success, string? errorMessage, User? user)> CreateUserAsync(User? user);
    Task<(bool success, string? errorMessage, User? user)> UpdateUserAsync(int id, User? user);
    Task<(bool success, string? errorMessage)> DeleteUserAsync(int id);
    Task<bool> UserExistsAsync(int id);
}
