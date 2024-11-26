using LabyrinthClient.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LabyrinthClient
{
    public partial class Message : Window
    {
        public enum DialogResult
        {
            Neutral,
            Confirm,
            Dismiss
        }

        public DialogResult UserDialogResult { get; private set; }

        public Message(string messageCode)
        {
            InitializeComponent();
            MessageTextBlock.Text = Messages.ResourceManager.GetString(messageCode);

            confirmButton.Visibility = Visibility.Collapsed;
            dismissButton.Content = Properties.Resources.AcceptButton;
        }

        public Message(string messageCode, string confirmButtonText, string dismissButtonText)
        {
            InitializeComponent();

            MessageTextBlock.Text = Messages.ResourceManager.GetString(messageCode);

            confirmButton.Content = confirmButtonText;

            dismissButton.Content = dismissButtonText;
        }

        private void ConfirmButtonIsPressed(object sender, RoutedEventArgs e)
        {
            UserDialogResult = DialogResult.Confirm;
            Close();
        }

        private void DismissButtonIsPressed(object sender, RoutedEventArgs e)
        {
            UserDialogResult = DialogResult.Dismiss;
            Close();
        }

        private void MessageWindowIsClosed(object sender, CancelEventArgs e)
        {
            if (UserDialogResult != DialogResult.Confirm && UserDialogResult != DialogResult.Dismiss)
            {
                UserDialogResult = DialogResult.Neutral;
            }
        }

    }
}

