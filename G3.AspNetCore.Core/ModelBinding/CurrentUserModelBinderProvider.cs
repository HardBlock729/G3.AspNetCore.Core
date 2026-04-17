using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace G3.AspNetCore.Core.ModelBinding;

/// <summary>
/// Model binder provider that creates <see cref="CurrentUserModelBinder{TUser}"/> instances
/// for parameters decorated with <see cref="FromCurrentUserAttribute"/>.
/// Register via: options.ModelBinderProviders.Insert(0, new CurrentUserModelBinderProvider())
/// </summary>
public sealed class CurrentUserModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.BindingInfo?.BindingSource != BindingSource.Custom)
        {
            return null;
        }

        var binderType = typeof(CurrentUserModelBinder<>).MakeGenericType(context.Metadata.ModelType);
        return new BinderTypeModelBinder(binderType);
    }
}
