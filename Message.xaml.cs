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
        public enum CustomDialogResult
        {
            Neutral,
            Confirm,
            Dismiss
        }

        public CustomDialogResult UserDialogResult { get; private set; }

        public Message(string message)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;

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

        public Message(string messageCode, string additionalText, string confirmButtonText, string dismissButtonText)
        {
            InitializeComponent();

            MessageTextBlock.Text = Messages.ResourceManager.GetString(messageCode) + " " + additionalText;

            confirmButton.Content = confirmButtonText;

            dismissButton.Content = dismissButtonText;
        }

        public Message(string messageCode, string additionalText)
        {
            InitializeComponent();
            MessageTextBlock.Text = Messages.ResourceManager.GetString(messageCode) + " " + additionalText;

            confirmButton.Visibility = Visibility.Collapsed;
            dismissButton.Content = Properties.Resources.AcceptButton;
        }

        private void ConfirmButtonIsPressed(object sender, RoutedEventArgs e)
        {
            UserDialogResult = CustomDialogResult.Confirm;
            Close();
        }

        private void DismissButtonIsPressed(object sender, RoutedEventArgs e)
        {
            UserDialogResult = CustomDialogResult.Dismiss;
            Close();
        }

        private void MessageWindowIsClosed(object sender, CancelEventArgs e)
        {
            if (UserDialogResult != CustomDialogResult.Confirm && UserDialogResult != CustomDialogResult.Dismiss)
            {
                UserDialogResult = CustomDialogResult.Neutral;
            }
        }

    }
}

