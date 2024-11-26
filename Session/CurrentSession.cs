using LabyrinthClient.UserManagementService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace LabyrinthClient.Session
{
    public static class CurrentSession
    {
        public static TransferUser CurrentUser { get; set; } = new TransferUser();

        public static BitmapImage ProfilePicture {  get; set; }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
