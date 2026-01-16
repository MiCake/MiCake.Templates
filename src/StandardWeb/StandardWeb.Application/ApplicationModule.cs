using MiCake.Audit.Core;
using MiCake.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using StandardWeb.Application.Audit;
using StandardWeb.Domain;

namespace StandardWeb.Application;

[RelyOn(typeof(DomainModule))]
public class ApplicationModule : MiCakeModule
{
    public override void ConfigureServices(ModuleConfigServiceContext context)
    {
        context.Services.AddScoped<IAuditProvider, CurrentUserAuditProvider>();
        base.ConfigureServices(context);
    }
}
