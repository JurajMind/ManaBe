using System;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

public class OwinContextExtensionsWrapper : IOwinContextExtensionsWrapper
{
    public T Get<T>(IOwinContext context)
    {
        return context.Get<T>();
    }

    public TManager GetUserManager<TManager>(IOwinContext context)
    {
        return context.GetUserManager<TManager>();
    }

    public IOwinContext Set<T>(IOwinContext context, T value)
    {
        return context.Set(value);
    }

    public TUser FindById<TUser, TKey>(TKey userId, IOwinContext context)
        where TUser : class, IUser<TKey> where TKey : IEquatable<TKey>
    {
        var manager = this.GetUserManager<UserManager<TUser, TKey>>(context);
        return manager.FindById(userId);
    }
}