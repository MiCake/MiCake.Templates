using MiCake.Cord.Paging;

namespace MiCakeTemplate.Domain.TodoContext.Repositories
{
    public interface ITodoItemRepo : IPaginationRepository<TodoItem, int>
    {
    }
}
