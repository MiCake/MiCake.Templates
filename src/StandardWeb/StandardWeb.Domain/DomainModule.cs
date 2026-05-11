using MiCake.Core.Modularity;
using StandardWeb.Common;

namespace StandardWeb.Domain;

[RelyOn(typeof(CommonModule))]
public class DomainModule : MiCakeModule
{
}
