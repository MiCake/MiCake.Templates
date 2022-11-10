using MiCake.AspNetCore.DataWrapper;
using MiCake.Cord.Paging;
using MiCakeTemplate.Api.DtoModels.Todo;
using MiCakeTemplate.Domain.TodoContext;
using MiCakeTemplate.Domain.TodoContext.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MiCakeTemplate.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoController : AppControllerBase<TodoController>
    {
        private readonly ITodoItemRepo _repo;

        public TodoController(ITodoItemRepo repo, ControllerInfrastructures infrastructures, ILoggerFactory loggerFactory) : base(infrastructures, loggerFactory)
        {
            _repo = repo;
        }

        [HttpPost("")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<TodoItemDto>))]
        public async Task<IActionResult> Create([FromBody] CreateTodoItemDto item)
        {
            var result = TodoItem.Create(item.Title!, item.Content);
            var record = await _repo.AddAndReturnAsync(result);

            return Ok(Mapper.Map<TodoItemDto>(record));
        }

        [HttpGet("/{id:int}")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<TodoItemDto>))]
        public async Task<IActionResult> GetItemDetail(int id)
        {
            var result = await _repo.FindAsync(id);

            return Ok(Mapper.Map<TodoItemDto>(result));
        }

        [HttpGet("/")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<PagingQueryResult<TodoItemDto>>))]
        public async Task<IActionResult> PagingQueryAllItems(int pageIndex, int pageSize)
        {
            var result = await _repo.PagingQueryAsync(new PaginationFilter(pageIndex, pageSize));

            return Ok(MapperPagingQueryResult<TodoItem, TodoItemDto>(result));
        }

        [HttpPut("{todoId:int}")]
        [ProducesResponseType(200, Type = typeof(ApiResponse<bool>))]
        public async Task<bool> ChangeTodo(int todoId, [FromBody] TodoItemDto item)
        {
            var todoItem = await _repo.FindAsync(todoId);
            if (todoItem is null)
            {
                return false;
            }

            todoItem.ChangeContent(item.Content);
            return true;
        }
    }
}
