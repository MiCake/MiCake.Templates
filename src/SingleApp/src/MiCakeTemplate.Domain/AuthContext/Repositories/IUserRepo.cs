using MiCake.DDD.Domain;

namespace MiCakeTemplate.Domain.AuthContext.Repositories
{
    public interface IUserRepo : IRepository<User, int>
    {
        Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default);

        Task<User?> GetByUsername(string username, CancellationToken cancellationToken = default);

        Task<User?> GetByPhone(string phoneNumber, CancellationToken cancellationToken = default);

        Task<bool> IsPhoneRegistered(string phoneNumber, CancellationToken cancellationToken = default);

        Task<bool> IsEmailRegistered(string email, CancellationToken cancellationToken = default);
    }
}
