namespace UserManagementAPI.Models;

public sealed record User
{
    public required int Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required int Age { get; init; }
    public required string Password { get; init; }
}