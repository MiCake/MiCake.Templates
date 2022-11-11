using MiCake.Core.Modularity;
using MiCakeTemplate.Domain;

namespace MiCakeTemplate.EFCore
{
    [RelyOn(typeof(DomainModule))]
    public class AppEFCoreModule : MiCakeModule
    {
        public override void ConfigServices(ModuleConfigServiceContext context)
        {
            base.ConfigServices(context);

            context.AutoRegisterRepositories(typeof(AppEFCoreModule).Assembly);
        }
    }
}
