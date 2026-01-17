using MiCake.Core.Modularity;
using StandardWeb.Domain;

namespace StandardWeb.EFCore;

[RelyOn(typeof(DomainModule), typeof(MiCake.EntityFrameworkCore.Modules.MiCakeEFCoreModule))]
public class EFCoreModule : MiCakeModule
{
    public override void ConfigureServices(ModuleConfigServiceContext context)
    {
        // Auto register repositories in the assembly.
        context.AutoRegisterRepositories(typeof(AppDbContext).Assembly);
    }
}
