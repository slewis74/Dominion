using System.Data.Entity;
using Autofac;
using Autofac.Builder;
using Dominion.Messages;

namespace Dominion.EntityFramework
{
    public static class ContainerBuilderExtensions
    {
        public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> WithEventPublisherOnMaterialized<TLimit, TReflectionActivatorData, TStyle>(this IRegistrationBuilder<TLimit, TReflectionActivatorData, TStyle> registration) where TReflectionActivatorData : ReflectionActivatorData
        {
            return registration.OnActivated(c =>
            {
                var dbContext = c.Instance as DbContext;
                if (dbContext == null)
                    return;
                var publisher = c.Context.Resolve<IMessagePublisher>();
                dbContext.OnObjectOfTypeMaterialized<IPublishDomainEvents>(t => t.SetPublisher(publisher));
            });
        }
    }
}