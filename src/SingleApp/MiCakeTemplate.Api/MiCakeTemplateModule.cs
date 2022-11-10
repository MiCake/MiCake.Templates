using MiCake.AspNetCore.Start;
using MiCake.AutoMapper;
using MiCake.Core.DependencyInjection;
using MiCake.Core.Modularity;
using MiCakeTemplate.EFCore;

namespace MiCakeTemplate.Api
{
    [RelyOn(typeof(AppEFCoreModule))]
    public class MiCakeTemplateModule : MiCakeStartModule
    {
    }
}
