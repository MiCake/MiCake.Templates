using MiCake.Core.Modularity;
using StandardWeb.Domain;

namespace StandardWeb.Application;

[RelyOn(typeof(DomainModule))]
public class ApplicationModule : MiCakeModule
{
    public override void ConfigureServices(ModuleConfigServiceContext context)
    {
    }
}
