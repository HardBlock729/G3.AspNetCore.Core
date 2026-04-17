using FluentValidation;
using FluentValidation.AspNetCore;
using G3.AspNetCore.Core.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace G3.AspNetCore.Core.Extensions;

/// <summary>
/// Extension methods for IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers FluentValidation auto-validation and loads all validators from the given assembly.
    /// </summary>
    public static IServiceCollection AddG3FluentValidation(
        this IServiceCollection services,
        Assembly validatorAssembly)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(validatorAssembly);
        return services;
    }

    /// <summary>
    /// Registers the <see cref="CurrentUserModelBinder{TUser}"/> open generic and the
    /// <see cref="ICurrentUserResolver{TUser}"/> implementation so that
    /// <see cref="FromCurrentUserAttribute"/> works for the given user type.
    /// Also inserts <see cref="CurrentUserModelBinderProvider"/> into MVC options.
    /// </summary>
    public static IServiceCollection AddG3CurrentUserModelBinding<TUser, TResolver>(
        this IServiceCollection services)
        where TUser : class
        where TResolver : class, ICurrentUserResolver<TUser>
    {
        services.AddScoped<ICurrentUserResolver<TUser>, TResolver>();
        services.AddTransient(typeof(CurrentUserModelBinder<>));
        services.Configure<MvcOptions>(options =>
            options.ModelBinderProviders.Insert(0, new CurrentUserModelBinderProvider()));
        return services;
    }
}
