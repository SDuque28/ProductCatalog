using System.Globalization;
using System.Text;
using ProductCatalog.Api.Exceptions;
using ProductCatalog.Api.Interfaces;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Repositories;

public class TxtUserRepository : IUserRepository
{
    private const char Separator = '|';
    private const int ExpectedColumnCount = 4;
    private static readonly SemaphoreSlim FileLock = new(1, 1);

    private readonly string _filePath;

    public TxtUserRepository(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "users.txt");
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await FileLock.WaitAsync();

        try
        {
            await EnsureFileExistsInternalAsync();
            var users = await LoadUsersInternalAsync();

            return users.FirstOrDefault(user =>
                string.Equals(user.Username, username.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await FileLock.WaitAsync();

        try
        {
            await EnsureFileExistsInternalAsync();
            var users = await LoadUsersInternalAsync();

            return users.FirstOrDefault(user =>
                string.Equals(user.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async Task AddAsync(User user)
    {
        await FileLock.WaitAsync();

        try
        {
            await EnsureFileExistsInternalAsync();

            var users = await LoadUsersInternalAsync();
            if (users.Any(existingUser =>
                    string.Equals(existingUser.Username, user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ConflictException("Username is already registered.");
            }

            if (users.Any(existingUser =>
                    string.Equals(existingUser.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ConflictException("Email is already registered.");
            }

            var line = string.Join(
                Separator,
                user.Username.Trim(),
                user.Email.Trim(),
                user.PasswordHash,
                user.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture));

            await File.AppendAllTextAsync(_filePath, $"{line}{Environment.NewLine}", Encoding.UTF8);
        }
        finally
        {
            FileLock.Release();
        }
    }

    public async Task<IReadOnlyCollection<User>> GetAllAsync()
    {
        await FileLock.WaitAsync();

        try
        {
            await EnsureFileExistsInternalAsync();
            return await LoadUsersInternalAsync();
        }
        finally
        {
            FileLock.Release();
        }
    }

    private async Task EnsureFileExistsInternalAsync()
    {
        var directoryPath = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (!File.Exists(_filePath))
        {
            await File.WriteAllTextAsync(_filePath, string.Empty, Encoding.UTF8);
        }
    }

    private async Task<IReadOnlyCollection<User>> LoadUsersInternalAsync()
    {
        var lines = await File.ReadAllLinesAsync(_filePath, Encoding.UTF8);
        var users = new List<User>();
        var existingUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            if (!TryParseUser(line, existingUsernames, existingEmails, out var user))
            {
                continue;
            }

            users.Add(user);
        }

        return users;
    }

    private static bool TryParseUser(
        string? line,
        ISet<string> existingUsernames,
        ISet<string> existingEmails,
        out User user)
    {
        user = null!;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var columns = line.Split(Separator);
        if (columns.Length < ExpectedColumnCount)
        {
            return false;
        }

        var username = columns[0].Trim();
        var email = columns[1].Trim();
        var passwordHash = columns[2].Trim();

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(passwordHash)
            || existingUsernames.Contains(username)
            || existingEmails.Contains(email)
            || !DateTime.TryParse(
                columns[3].Trim(),
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out var createdAtUtc))
        {
            return false;
        }

        existingUsernames.Add(username);
        existingEmails.Add(email);

        user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAtUtc = createdAtUtc
        };

        return true;
    }
}
