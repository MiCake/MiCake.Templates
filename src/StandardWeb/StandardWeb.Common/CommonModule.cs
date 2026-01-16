using MiCake.Core;
using MiCake.Core.Modularity;

namespace StandardWeb.Common;

[RelyOn(typeof(MiCakeRootModule))]
public class CommonModule : MiCakeModule
{
    public override void ConfigureServices(ModuleConfigServiceContext context)
    {
    }
}
