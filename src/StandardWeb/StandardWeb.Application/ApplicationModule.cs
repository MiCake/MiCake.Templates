using MiCake.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using StandardWeb.Common.Auth;
using StandardWeb.Domain;

namespace StandardWeb.Application;

[RelyOn(typeof(DomainModule))]
public class ApplicationModule : MiCakeModule
{
    public override Task ConfigServices(ModuleConfigServiceContext context)
    {
        context.Services.AddDistributedMemoryCache();

        context.Services.AddOptions<AESEncryptionOptions>()
            .BindConfiguration("AESEncryption")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "AESEncryption:Key must be configured.")
            .ValidateOnStart();

        return base.ConfigServices(context);
    }
}
