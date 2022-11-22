using MiCake.DDD.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiCakeTemplate.EFCore.Common
{
    internal static class DomainObjectRelationshipExtension
    {
        // this extend method used to config efcore relationship between aggregate root and entity.

        /// <summary>
        /// Use to mapping relationship between aggregate root and entity which have a one-to-one relationship. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="ownedNavigation"></param>
        public static void MapDomainEntityOwnsOneRelationship<T, U>(this OwnedNavigationBuilder<T, U> ownedNavigation)
            where T : class, IAggregateRoot
            where U : class, IEntity
        {
            var dependAggName = typeof(T).Name;
            var entityTypeName = typeof(U).Name;

            ownedNavigation.ToTable(entityTypeName);
            ownedNavigation.HasKey("Id");
            ownedNavigation.WithOwner().HasForeignKey($"{dependAggName}Id");
        }

        /// <summary>
        ///  Use to mapping relationship between aggregate root and value object which have a one-to-one relationship. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="ownedNavigation"></param>
        public static void MapDomainValueObjectOwnsOneRelationship<T, U>(this OwnedNavigationBuilder<T, U> ownedNavigation)
           where T : class, IAggregateRoot
           where U : class, IValueObject
        {
            ownedNavigation.ToJson();
        }

        /// <summary>
        /// Use to mapping relationship between aggregate root and value object which have a one-to-many relationship. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="ownedNavigation"></param>
        public static void MapDomainValueObjectOwnsManyRelationship<T, U>(this OwnedNavigationBuilder<T, U> ownedNavigation)
           where T : class, IAggregateRoot
           where U : class, IValueObject
        {
            ownedNavigation.ToJson();
        }
    }
}
