using MiCake.Core.Modularity;
using RBACWeb.Common;

namespace RBACWeb.Domain;

[RelyOn(typeof(CommonModule))]
public class DomainModule : MiCakeModule
{
}
