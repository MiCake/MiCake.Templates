using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiCakeTemplate.Util.Validation
{
    public static class ValidationHelper
    {
        public static bool IsPhoneNumber(string phone)
        {
            return Regex.IsMatch(phone, @"/^(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\d{8}$/");
        }

        public static bool IsEmail(string email)
        {
            return Regex.IsMatch(email, @"/^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/");
        }
    }
}
