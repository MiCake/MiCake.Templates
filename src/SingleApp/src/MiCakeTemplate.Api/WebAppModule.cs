using MiCake.AspNetCore.Start;
using MiCake.AutoMapper;
using MiCake.Core.Modularity;
using MiCakeTemplate.EFCore;

namespace MiCakeTemplate.Api
{
    [RelyOn(typeof(AppEFCoreModule))]
    [UseAutoMapper]
    public class WebAppModule : MiCakeStartModule
    {
    }
}
