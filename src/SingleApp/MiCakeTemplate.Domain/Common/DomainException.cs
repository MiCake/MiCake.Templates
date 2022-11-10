using MiCake.Core;
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
    /// Represent an exception which happened in domain business.
    /// </summary>
    public class DomainException : PureException
    {
        public DomainException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Create a <see cref="DomainException"/> from <see cref="ErrorDefinition"/>
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static DomainException Create(ErrorDefinition error)
        {
            var ex = new DomainException(error.Message)
            {
                Code = $"{error.Code}"
            };

            return ex;
        }
    }
}
