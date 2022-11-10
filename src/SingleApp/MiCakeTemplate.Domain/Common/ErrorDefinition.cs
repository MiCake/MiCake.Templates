using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Tips: This folder is used to store some common base classes
 */

namespace MiCakeTemplate.Domain
{
    /// <summary>
    /// Represent a defined error.
    /// </summary>
    public struct ErrorDefinition
    {
        public string Code { get; set; }
        public string Message { get; set; }

        public ErrorDefinition(string code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
