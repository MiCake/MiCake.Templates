using MiCake.Core.Modularity;
using MiCakeTemplate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
