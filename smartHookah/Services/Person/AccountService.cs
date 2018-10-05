namespace smartHookah.Services.Person
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.OAuth;

    using Newtonsoft.Json.Linq;

    using smartHookah.Models;

    public class AccountService : IAccountService
    {
        public async Task<JObject> GenerateLocalAccessTokenResponse(ApplicationUser user, ApplicationUserManager userManager)
        {
            var tokenExpiration = TimeSpan.FromDays(1);

            ClaimsIdentity identity =
                await userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);

            var props = new AuthenticationProperties()
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
            };

            var ticket = new AuthenticationTicket(identity, props);

            var accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
            var refreshToken = string.Empty;
            using (AuthRepository _repo = new AuthRepository())
            {
                var refreshTokenId = Guid.NewGuid().ToString("n");
                var token = new RefreshToken()
                {
                    Id = Helper.GetHash(refreshTokenId),
                    ClientId = "test",
                    Subject = identity.Name,
                    IssuedUtc = DateTime.UtcNow,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(Convert.ToDouble(2000))
                };

                token.ProtectedTicket = Startup.OAuthServerOptions.RefreshTokenFormat.Protect(ticket);
                var result = await _repo.AddRefreshToken(token);
                if (result)
                {
                    refreshToken = refreshTokenId;
                }
            }

            JObject tokenResponse = new JObject(
                new JProperty("userName", user.DisplayName),
                new JProperty("access_token", accessToken),
                new JProperty("refresh_token", refreshToken),
                new JProperty("token_type", "bearer"),
                new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString()),
                new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
                new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString()));

            return tokenResponse;
        }
    }

    public interface IAccountService
    {
        Task<JObject> GenerateLocalAccessTokenResponse(ApplicationUser user, ApplicationUserManager userManager);
    }
}