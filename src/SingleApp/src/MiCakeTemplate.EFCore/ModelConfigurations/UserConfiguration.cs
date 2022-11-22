using MiCakeTemplate.Domain.AuthContext;
using MiCakeTemplate.EFCore.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiCakeTemplate.EFCore.ModelConfigurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(s => s.UserName);

            // for aggregate root, you should use owns*** to config it.
            // more information about efcore owned-entities, you can see https://learn.microsoft.com/zh-cn/ef/core/modeling/owned-entities.

            builder.OwnsMany(s => s.UserIdentifications, i =>
            {
                i.HasIndex(s => s.Value);
            });

            builder.OwnsOne(s => s.UserToken, i =>
            {
                i.MapDomainEntityOwnsOneRelationship();
            });
        }
    }
}
