using MiCake.Core.Modularity;
using StandardWeb.Domain;

namespace StandardWeb.Application;

[RelyOn(typeof(DomainModule))]
public class ApplicationModule : MiCakeModule
{
}
