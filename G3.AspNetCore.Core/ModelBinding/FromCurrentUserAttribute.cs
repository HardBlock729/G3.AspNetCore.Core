using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace G3.AspNetCore.Core.ModelBinding;

/// <summary>
/// Indicates that the parameter should be bound to the current authenticated user.
/// The user is resolved via <see cref="ICurrentUserResolver{TUser}"/> registered in DI.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromCurrentUserAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource => BindingSource.Custom;
}
