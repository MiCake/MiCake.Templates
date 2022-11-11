using MiCake.AspNetCore.Start;
using MiCake.Core.Modularity;
using MiCakeTemplate.EFCore;

namespace MiCakeTemplate.Api
{
    [RelyOn(typeof(AppEFCoreModule))]
    public class MiCakeTemplateModule : MiCakeStartModule
    {
    }
}
