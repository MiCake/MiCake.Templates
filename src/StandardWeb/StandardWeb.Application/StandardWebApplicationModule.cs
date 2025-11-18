using MiCake.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StandardWeb.Domain;

namespace StandardWeb.Application;

[RelyOn(typeof(DomainModule))]
public class StandardWebApplicationModule : MiCakeModule
{
    public override Task PreConfigServices(ModuleConfigServiceContext context)
    {
        // check if the AESEncryption section has been configured in appsettings
        var configuration = context.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var aesSection = configuration.GetSection("AESEncryption");
        if (!aesSection.Exists())
        {
            throw new Exception("AESEncryption configuration section is missing in appsettings.");
        }

        return base.PreConfigServices(context);
    }

    public override Task ConfigServices(ModuleConfigServiceContext context)
    {
        context.Services.AddDistributedMemoryCache();
        return base.ConfigServices(context);
    }
}
