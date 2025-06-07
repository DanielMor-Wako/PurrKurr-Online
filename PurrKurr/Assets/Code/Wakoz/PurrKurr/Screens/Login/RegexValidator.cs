using System;
using System.Text.RegularExpressions;

namespace Code.Wakoz.PurrKurr.Screens.Login
{
    public sealed class RegexValidator
    {
        public bool IsValidEmail(string emailAddress) {

            if (string.IsNullOrWhiteSpace(emailAddress)) {
                return false;
            }

            var emailPattern = @"[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
            
            return Regex.IsMatch(emailAddress, emailPattern);
        }

        public bool IsValidUsername(string name) {
            
            if (string.IsNullOrEmpty(name) || name.Length < 3 || name.Length > 50) {
                return false;
            }

            var pattern = @"^[\p{L}\p{N}~!@#$%^&()+}|_.-]*$";

            return Regex.IsMatch(name, pattern);
        }
    }
}