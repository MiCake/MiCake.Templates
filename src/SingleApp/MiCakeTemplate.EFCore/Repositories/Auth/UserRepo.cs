using MiCake.EntityFrameworkCore.Repository;
using MiCakeTemplate.Domain.AuthContext;
using MiCakeTemplate.Domain.AuthContext.Enums;
using MiCakeTemplate.Domain.AuthContext.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MiCakeTemplate.EFCore.Repositories.Auth
{
    internal class UserRepo : EFRepository<AppDbContext, User, int>, IUserRepo
    {
        public UserRepo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default)
        {
            return FindUserByUserIdentification(email, UserIdentificationType.Email, cancellationToken);
        }

        public Task<User?> GetByPhone(string phoneNumber, CancellationToken cancellationToken = default)
        {
            return FindUserByUserIdentification(phoneNumber, UserIdentificationType.Phone, cancellationToken);
        }

        public Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default)
        {
            return DbSet.FirstOrDefaultAsync(s => s.UserName == username, cancellationToken: cancellationToken);
        }

        public async Task<bool> IsEmailRegistered(string email, CancellationToken cancellationToken = default)
        {
            var record = await GetByEmail(email, cancellationToken);
            return record is not null;
        }

        public async Task<bool> IsPhoneRegistered(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var record = await GetByPhone(phoneNumber, cancellationToken);
            return record is not null;
        }

        private Task<User?> FindUserByUserIdentification(string value, UserIdentificationType type, CancellationToken cancellationToken = default)
        {
            return DbSet.FirstOrDefaultAsync(s => s.UserIdentifications.Any(j => j.Value == value && j.Type == type), cancellationToken: cancellationToken);
        }
    }
}
