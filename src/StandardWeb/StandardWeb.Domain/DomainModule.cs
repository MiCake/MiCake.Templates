using MiCake.Core.Modularity;
using MiCake.EntityFrameworkCore.Modules;

namespace StandardWeb.Domain;

[RelyOn(typeof(MiCakeEFCoreModule))]
public class DomainModule : MiCakeModule
{
    public override Task ConfigServices(ModuleConfigServiceContext context)
    {
        context.AutoRegisterRepositories(typeof(AppDbContext).Assembly);

        return base.ConfigServices(context);
    }
}
