using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Dominion.EntityFramework
{
    public static class DbContextExtensions
    {
        public static void OnObjectOfTypeMaterialized<T>(this DbContext context, Action<T> callback)
            where T : class
        {
            (context as IObjectContextAdapter).ObjectContext.ObjectMaterialized += (sender, args) =>
            {
                var t = args.Entity as T;
                if (t != null)
                    callback(t);
            };
        }
    }
}