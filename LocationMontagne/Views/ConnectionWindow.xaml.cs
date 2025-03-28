using LocationMontagne.Models;
using LocationMontagne.ViewModels;
using LocationMontagne.Views;
using System;
using System.Windows;


namespace LocationMontagne.Views
{
    /// <summary>
    /// Logique d'interaction pour ConnectionWindow.xaml
    /// </summary>
    public partial class ConnectionWindow : Window
    {
        private readonly UserVM userVM;

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre de connexion
        /// avec le ViewModel d'utilisateur nécessaire.
        /// </summary>
        public ConnectionWindow()
        {
            InitializeComponent();
            userVM = new UserVM();
        }

        /// <summary>
        /// Gère le clic sur le bouton "Connexion".
        /// Vérifie les identifiants de l'utilisateur et redirige vers la fenêtre appropriée en fonction du type d'utilisateur.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
                {
                    MessageBox.Show("Veuillez saisir un identifiant.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Veuillez saisir un mot de passe.");
                    return;
                }

                var userToConnect = new User
                {
                    login = LoginTextBox.Text,
                    password = PasswordBox.Password
                };

                User connectedUser = userVM.connexion(userToConnect);

                if (connectedUser != null && connectedUser.id != 0)
                {
                    if (connectedUser.estEmploye)
                    {
                        EmployeeDashboardWindow employeeDashboard = new EmployeeDashboardWindow();
                        employeeDashboard.Show();
                        this.Close();
                    }
                    else
                    {
                        MainWindow mainWindow = new MainWindow(connectedUser);
                        mainWindow.Show();
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Identifiant ou mot de passe incorrect.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la connexion : {ex.Message}");
            }
        }

        /// <summary>
        /// Gère la navigation vers la fenêtre d'inscription.
        /// Ouvre la fenêtre d'inscription et ferme la fenêtre actuelle.
        /// </summary>
        /// <param name="sender">Le contrôle qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        public void NavigateToInscription(object sender, RoutedEventArgs e)
        {
            InscriptionWindow inscriptionWindow = new InscriptionWindow();
            inscriptionWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Gère la navigation vers la fenêtre principale sans connexion.
        /// Ouvre la fenêtre principale en mode invité et ferme la fenêtre actuelle.
        /// </summary>
        /// <param name="sender">Le contrôle qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        public void NavigateToMain(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
