using MiCake.EntityFrameworkCore.Repository;
using MiCakeTemplate.Domain.TodoContext;
using MiCakeTemplate.Domain.TodoContext.Repositories;

namespace MiCakeTemplate.EFCore.Repositories.Todo
{
    internal class TodoItemRepo : EFPaginationRepository<AppDbContext, TodoItem, int>, ITodoItemRepo
    {
        public TodoItemRepo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
