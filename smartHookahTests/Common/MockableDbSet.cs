using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smartHookahTests.Common
{
    using System.Data.Entity;
    using System.Linq.Expressions;

    public abstract class MockableDbSetWithExtensions<T> : DbSet<T>
        where T : class
    {
        public abstract void AddOrUpdate(params T[] entities);
        public abstract void AddOrUpdate(Expression<Func<T, object>>
                                             identifierExpression, params T[] entities);
    }
}
