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
            bool isValid = true;
            if (string.IsNullOrEmpty(username) && !Regex.IsMatch(username, _usernamePattern))
            {
                MessageBox.Show("El nombre de usuario debe tener entre 3 y 50 caracteres y solo puede contener letras y números.");
                isValid = false;
            }
            return isValid;
        }

        public static bool IsValidPassword(string password)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(password) && !Regex.IsMatch(password, _passwordPattern))
            {
                MessageBox.Show("La contraseña debe tener entre 8 y 64 caracteres, e incluir al menos una letra mayúscula, un número y un carácter especial.");
                isValid = false;
            }
            return isValid;
        }

        public static bool IsValidEmail(string email)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(email) && !Regex.IsMatch(email, _emailPattern))
            {
                MessageBox.Show("El correo electrónico no es válido. Asegúrate de que esté en el formato correcto.");
                isValid = false;
            }
            return isValid;
        }
    }
}

