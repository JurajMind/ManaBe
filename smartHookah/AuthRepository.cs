using smartHookah.Models.Db;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace smartHookah
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Threading.Tasks;

    public class AuthRepository : IDisposable
    {
        private readonly SmartHookahContext _ctx;

        private readonly UserManager<ApplicationUser> _userManager;

        public AuthRepository()
        {
            this._ctx = new SmartHookahContext();
            this._userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this._ctx));
        }

        public void Dispose()
        {
            this._ctx.Dispose();
            this._userManager.Dispose();
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await this._userManager.FindAsync(userName, password);

            return user;
        }

        public Client FindClient(string clientId)
        {
            var client = _ctx.Clients.Find(clientId);

            return client;
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            var matchUser = await this._userManager.FindByEmailAsync(userModel.Email);
            var person = new Person();
            if (matchUser != null)
            {
                person = matchUser.Person;
            }
            else
            {
                person = new Person();
            }
            var user = new ApplicationUser
            {
                UserName = userModel.UserName,
                Email = userModel.Email,

            };

            var result = await this._userManager.CreateAsync(user, userModel.Password);

            return result;
        }

        public async Task<bool> AddRefreshToken(RefreshToken token)
        {

            var existingTokens = await
                _ctx.RefreshTokens.Where(r => r.Subject == token.Subject && r.ClientId == token.ClientId).ToListAsync();

            foreach (var existingToken in existingTokens)
            {
                if (existingToken.ExpiresUtc < DateTime.UtcNow)
                {
                    await RemoveRefreshToken(existingToken);
                }
            }

            _ctx.RefreshTokens.Add(token);
            try
            {
                return await _ctx.SaveChangesAsync() > 0;
            }
            catch (DbEntityValidationException e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

            if (refreshToken != null)
            {
                _ctx.RefreshTokens.Remove(refreshToken);
                return await _ctx.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
            _ctx.RefreshTokens.Remove(refreshToken);
            return await _ctx.SaveChangesAsync() > 0;
        }

        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _ctx.RefreshTokens.FindAsync(refreshTokenId);

            return refreshToken;
        }

        public List<RefreshToken> GetAllRefreshTokens()
        {
            return _ctx.RefreshTokens.ToList();
        }
    }

    public class RefreshToken
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Subject { get; set; }
        [Required]
        [MaxLength(50)]
        public string ClientId { get; set; }
        public DateTime IssuedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        [Required]
        public string ProtectedTicket { get; set; }
    }

    public class Client
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Secret { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public ApplicationTypes ApplicationType { get; set; }
        public bool Active { get; set; }
        public int RefreshTokenLifeTime { get; set; }
        [MaxLength(100)]
        public string AllowedOrigin { get; set; }
    }

    public enum ApplicationTypes
    {
        NativeConfidential
    }
}

public class UserModel
{
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Display(Name = "User name")]
    public string UserName { get; set; }

    [Required]
    [Display(Name = "User name")]
    public string Email { get; set; }
}
