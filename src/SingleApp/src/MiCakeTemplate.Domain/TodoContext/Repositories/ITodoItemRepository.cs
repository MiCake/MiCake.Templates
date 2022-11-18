using MiCake.Cord.Paging;

namespace MiCakeTemplate.Domain.TodoContext.Repositories
{
    public interface ITodoItemRepository : IPaginationRepository<TodoItem, int>
    {
    }
}
