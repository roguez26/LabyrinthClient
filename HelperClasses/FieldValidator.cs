using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelperClasses
{
    public class FieldValidator
    {
        private static string _usernamePattern = @"^(?=.{3,50}$)[a-zA-Z0-9]+$";
        private static string _passwordPattern = @"^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*()_+[\]{};':""\\|,.<>/?-]).{8,64}$";
        private static string _emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username) || !Regex.IsMatch(username, _usernamePattern))
            {
                throw new ArgumentException("FailInvalidUsernameMessage");
            }
            return true;
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || !Regex.IsMatch(password, _passwordPattern))
            {
                throw new ArgumentException("FailInvalidPasswordMessage");
            }
            return true;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, _emailPattern))
            {
                throw new ArgumentException("FailInvalidEmailMessage");
            }
            return true;
        }
    }
}

