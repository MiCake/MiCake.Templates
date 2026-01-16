using System;

namespace StandardWeb.Domain.Common;

public interface IHasModifiedBy
{
    long? ModifiedBy { get; set; }
}
