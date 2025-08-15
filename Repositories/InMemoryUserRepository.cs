using System.Collections.Concurrent;
using UserManagementAPI.Models;
using UserManagementAPI.Interfaces;

namespace UserManagementAPI.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<int, User> _users;
    private int _nextUserId;

    public InMemoryUserRepository()
    {
        _users = new ConcurrentDictionary<int, User>
        {
            [1] = new User { Id = 1, Username = "Alice", Email = "alice@example.com", Age = 30, Password = "password123" },
            [2] = new User { Id = 2, Username = "Bob", Email = "bob@example.com", Age = 25, Password = "password123" },
            [3] = new User { Id = 3, Username = "Charlie", Email = "charlie@example.com", Age = 35, Password = "password123" }
        };
        _nextUserId = _users.Count; // Start at 3, so next increment gives 4
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return Task.FromResult(_users.Values.AsEnumerable());
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(User user)
    {
        int newId = Interlocked.Increment(ref _nextUserId); 
        var newUser = user with { Id = newId };
        _users.TryAdd(newId, newUser);
        return Task.FromResult(newUser);
    }

    public Task<User?> UpdateUserAsync(int id, User user)
    {
        if (!_users.TryGetValue(id, out var existingUser))
            return Task.FromResult<User?>(null);

        var updatedUser = user with { Id = id };
        return Task.FromResult(_users.TryUpdate(id, updatedUser, existingUser) ? updatedUser : null);
    }

    public Task<bool> DeleteUserAsync(int id)
    {
        return Task.FromResult(_users.TryRemove(id, out _));
    }

    public Task<bool> UserExistsAsync(int id)
    {
        return Task.FromResult(_users.ContainsKey(id));
    }
}