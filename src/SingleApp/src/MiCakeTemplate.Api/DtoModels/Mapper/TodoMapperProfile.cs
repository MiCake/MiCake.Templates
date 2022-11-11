using AutoMapper;
using MiCakeTemplate.Api.DtoModels.Todo;
using MiCakeTemplate.Domain.TodoContext;

namespace MiCakeTemplate.Api.DtoModels.Mapper
{
    public class TodoMapperProfile : Profile
    {
        public TodoMapperProfile()
        {
            CreateMap<TodoItem, TodoItemDto>();
        }
    }
}
