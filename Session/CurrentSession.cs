using LabyrinthClient.UserManagementService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LabyrinthClient.Session
{
    public static class CurrentSession
    {
        public static TransferUser CurrentUser { get; set; }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
