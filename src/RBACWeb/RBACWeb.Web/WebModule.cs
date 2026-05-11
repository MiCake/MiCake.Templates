using MiCake.AspNetCore.Modules;
using MiCake.Core.Modularity;
using RBACWeb.Application;

namespace RBACWeb.Web;

[RelyOn(typeof(ApplicationModule), typeof(MiCakeAspNetCoreModule))]
public class WebModule : MiCakeModule
{

}
