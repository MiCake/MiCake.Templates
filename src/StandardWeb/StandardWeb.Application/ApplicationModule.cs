using MiCake.Audit.Core;
using MiCake.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using StandardWeb.Application.Audit;
using StandardWeb.EFCore;

namespace StandardWeb.Application;

[RelyOn(typeof(EFCoreModule))]
public class ApplicationModule : MiCakeModule
{
    public override void ConfigureServices(ModuleConfigServiceContext context)
    {
        context.Services.AddScoped<IAuditProvider, CurrentUserAuditProvider>();
        base.ConfigureServices(context);
    }
}
