using MiCake.AspNetCore.Modules;
using MiCake.Core.Modularity;
using StandardWeb.Application;

namespace StandardWeb.Web;

[RelyOn(typeof(ApplicationModule), typeof(MiCakeAspNetCoreModule))]
public class WebModule : MiCakeModule
{

}
