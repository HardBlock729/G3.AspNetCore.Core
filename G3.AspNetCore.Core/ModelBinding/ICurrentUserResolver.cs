using System.Threading;
using System.Threading.Tasks;

namespace G3.AspNetCore.Core.ModelBinding;

/// <summary>
/// Resolves the current authenticated user from their identity string (e.g. Cognito sub claim).
/// Implement this interface and register it in DI to use <see cref="FromCurrentUserAttribute"/>.
/// </summary>
/// <typeparam name="TUser">The application's current-user model type.</typeparam>
public interface ICurrentUserResolver<TUser>
{
    /// <summary>
    /// Resolves the user record for the given identity string.
    /// Return null if no matching user is found (binding will fail with 401).
    /// </summary>
    Task<TUser?> ResolveAsync(string userId, CancellationToken cancellationToken);
}
