using MiCake.DDD.Domain;
using MiCake.Identity;
using MiCake.Identity.Authentication;
using MiCake.Identity.Authentication.JwtToken;
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
    // If you don't want to use the jwt validation that comes with MiCake, you can remove the class.

    public abstract class AppUser<TKey> : AggregateRoot<TKey>, IMiCakeUser<TKey> where TKey : notnull
    {
        [JwtClaim(ClaimName = MiCakeClaimTypes.UserId)]
        protected TKey IdForClaim => this.Id;
    }
}
