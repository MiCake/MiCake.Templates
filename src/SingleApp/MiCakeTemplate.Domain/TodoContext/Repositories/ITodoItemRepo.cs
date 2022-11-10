using MiCake.Cord.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiCakeTemplate.Domain.TodoContext.Repositories
{
    public interface ITodoItemRepo : IPaginationRepository<TodoItem, int>
    {
    }
}
