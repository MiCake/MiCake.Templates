using MiCake.EntityFrameworkCore.Repository;
using MiCakeTemplate.Domain.TodoContext;
using MiCakeTemplate.Domain.TodoContext.Repositories;

namespace MiCakeTemplate.EFCore.Repositories.Todo
{
    internal class TodoItemRepository : EFPaginationRepository<AppDbContext, TodoItem, int>, ITodoItemRepository
    {
        public TodoItemRepository(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
