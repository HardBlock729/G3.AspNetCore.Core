using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace G3.AspNetCore.Core.ModelBinding;

/// <summary>
/// Model binder that resolves the current authenticated user via <see cref="ICurrentUserResolver{TUser}"/>.
/// Register the open generic type in DI: services.AddTransient(typeof(CurrentUserModelBinder&lt;&gt;))
/// </summary>
public sealed class CurrentUserModelBinder<TUser> : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var user = bindingContext.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        var userId = user.FindFirst("sub")?.Value
                     ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        var resolver = bindingContext.HttpContext.RequestServices
            .GetRequiredService<ICurrentUserResolver<TUser>>();

        var resolved = await resolver.ResolveAsync(userId, bindingContext.HttpContext.RequestAborted);
        if (resolved is null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        bindingContext.Result = ModelBindingResult.Success(resolved);
    }
}
