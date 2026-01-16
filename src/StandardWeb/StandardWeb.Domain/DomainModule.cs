using MiCake.Core.Modularity;
using MiCake.EntityFrameworkCore.Modules;
using StandardWeb.Common;

namespace StandardWeb.Domain;

[RelyOn(typeof(MiCakeEFCoreModule), typeof(CommonModule))]
public class DomainModule : MiCakeModule
{
    public override void ConfigureServices(ModuleConfigServiceContext context)
    {
        // Auto register repositories in the assembly.
        context.AutoRegisterRepositories(typeof(AppDbContext).Assembly);
    }
}
