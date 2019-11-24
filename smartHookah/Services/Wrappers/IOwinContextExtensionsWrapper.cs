using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using System;

public interface IOwinContextExtensionsWrapper
{
    T Get<T>(IOwinContext context);

    TManager GetUserManager<TManager>(IOwinContext context);

    IOwinContext Set<T>(IOwinContext context, T value);

    TUser FindById<TUser, TKey>(TKey userId, IOwinContext context)
        where TUser : class, IUser<TKey> where TKey : IEquatable<TKey>;
}