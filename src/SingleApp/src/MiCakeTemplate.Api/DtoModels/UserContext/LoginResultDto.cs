namespace MiCakeTemplate.Api.DtoModels.UserContext
{
    public class LoginResultDto
    {
        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }
    }
}
