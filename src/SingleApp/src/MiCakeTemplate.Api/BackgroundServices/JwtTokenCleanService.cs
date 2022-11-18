using MiCake.Identity.Authentication.JwtToken;

namespace MiCakeTemplate.Api.BackgroundServices
{
    /*
     *  Because we are using MiCake's jwt default implementation, all refresh tokens will be kept in memory and not actively released, 
     *  So we need a scheduled task to refresh it and avoid memory overflow.
     * 
     */

    public class JwtTokenCleanService : BackgroundService
    {
        private readonly IRefreshTokenStore _tokenStore;

        public JwtTokenCleanService(IRefreshTokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
