using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiCakeTemplate.Domain.TodoContext
{
    public class TodoItem : AuditTimeAggregateRoot
    {
        public string? Title { get; protected set; }

        public string? Content { get; protected set; }

        public static TodoItem Create(string title, string? content)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new DomainException("Todo item title is empty.");
            }

            return new TodoItem()
            {
                Title = title,
                Content = content
            };
        }

        public void ChangeContent(string? content) => Content = content;
    }
}
