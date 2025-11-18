using MiCake.AspNetCore.Modules;
using MiCake.Core.Modularity;
using StandardWeb.Application;

namespace StandardWeb.Web;

[RelyOn(typeof(StandardWebApplicationModule), typeof(MiCakeAspNetCoreModule))]
public class AppModule : MiCakeModule
{

}
