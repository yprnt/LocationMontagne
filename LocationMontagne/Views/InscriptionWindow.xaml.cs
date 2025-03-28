using LocationMontagne.Models;
using LocationMontagne.ViewModels;
using LocationMontagne.Views;
using System;
using System.Text.RegularExpressions;
using System.Windows;


namespace LocationMontagne.Views
{
    /// <summary>
    /// Logique d'interaction pour InscriptionWindow.xaml
    /// </summary>
    public partial class InscriptionWindow : Window
    {
        private readonly VilleVM villeVM;
        private readonly UserVM userVM;

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre d'inscription
        /// avec les ViewModels nécessaires et charge la liste des villes.
        /// </summary>
        public InscriptionWindow()
        {
            InitializeComponent();
            villeVM = new VilleVM();
            userVM = new UserVM();
            DataContext = villeVM;
            ChargerVilles();
        }

        /// <summary>
        /// Charge la liste des villes dans le ComboBox.
        /// </summary>
        private void ChargerVilles()
        {
            CityComboBox.ItemsSource = villeVM.villes;
            CityComboBox.DisplayMemberPath = null;
            CityComboBox.SelectedValuePath = "idVille";
        }

        /// <summary>
        /// Gère le clic sur le bouton "Ajouter Ville".
        /// Ouvre une fenêtre d'ajout de ville et met à jour la sélection si une nouvelle ville est créée.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void AjouterVille_Click(object sender, RoutedEventArgs e)
        {
            var addCityPopup = new AddCityWindow(this, villeVM);
            if (addCityPopup.ShowDialog() == true)
            {
                CityComboBox.SelectedValue = addCityPopup.NouvelleVille.idVille;
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton "S'inscrire".
        /// Valide toutes les entrées utilisateur et crée un nouveau compte utilisateur si les validations sont réussies.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void Inscription_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CityComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Veuillez sélectionner une ville.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!BirthDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Veuillez sélectionner une date de naissance.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string nom = LastNameTextBox.Text;
                string prenom = FirstNameTextBox.Text;
                string email = EmailTextBox.Text;
                string login = LoginTextBox.Text;
                string password = PasswordBox.Password;
                DateTime dateNaissance = BirthDatePicker.SelectedDate.Value;
                string adresse = AddressTextBox.Text;

                if (!ValidateName(nom))
                {
                    MessageBox.Show("Le nom doit contenir uniquement des lettres et avoir une longueur maximale de 40 caractères.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateName(prenom))
                {
                    MessageBox.Show("Le prénom doit contenir uniquement des lettres et avoir une longueur maximale de 40 caractères.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateEmail(email))
                {
                    MessageBox.Show("L'adresse email doit être sous la forme 'adresse@gmail.fr' et avoir une longueur maximale de 40 caractères.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateLogin(login))
                {
                    MessageBox.Show("L'identifiant doit contenir uniquement des lettres et des chiffres et avoir une longueur maximale de 40 caractères.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidatePassword(password))
                {
                    MessageBox.Show("Le mot de passe doit contenir au moins 12 caractères, 2 majuscules, 2 minuscules, 2 chiffres et 2 caractères spéciaux (.-+#[]*&<>\\).", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateBirthDate(dateNaissance))
                {
                    MessageBox.Show("La date de naissance doit être valide et vous devez avoir au moins 18 ans.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ValidateAddress(adresse))
                {
                    MessageBox.Show("L'adresse doit contenir uniquement des lettres et des chiffres et avoir une longueur maximale de 80 caractères.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newUser = new User
                {
                    nom = nom,
                    prenom = prenom,
                    email = email,
                    login = login,
                    password = password,
                    dateNaissance = dateNaissance,
                    adresse = adresse,
                    estEmploye = false
                };

                int idVille = (int)CityComboBox.SelectedValue;

                if (userVM.inscription(newUser, idVille))
                {
                    MessageBox.Show("Inscription réussie !", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    var connexionWindow = new ConnectionWindow();
                    connexionWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("L'inscription a échoué. Veuillez réessayer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Valide que le nom ou prénom contient uniquement des lettres et respecte la longueur maximale.
        /// </summary>
        /// <param name="name">Le nom ou prénom à valider</param>
        /// <returns>True si le nom est valide, sinon False</returns>
        private bool ValidateName(string name)
        {
            return Regex.IsMatch(name, @"^[a-zA-Z]{1,40}$");
        }

        /// <summary>
        /// Valide que l'adresse email est au format correct.
        /// </summary>
        /// <param name="email">L'adresse email à valider</param>
        /// <returns>True si l'email est valide, sinon False</returns>
        private bool ValidateEmail(string email)
        {
            return Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2}$");
        }

        /// <summary>
        /// Valide que l'identifiant contient uniquement des lettres et des chiffres et respecte la longueur maximale.
        /// </summary>
        /// <param name="login">L'identifiant à valider</param>
        /// <returns>True si l'identifiant est valide, sinon False</returns>
        private bool ValidateLogin(string login)
        {
            return Regex.IsMatch(login, @"^[a-zA-Z0-9]{1,40}$");
        }

        /// <summary>
        /// Valide que le mot de passe respecte les critères de sécurité spécifiés.
        /// </summary>
        /// <param name="password">Le mot de passe à valider</param>
        /// <returns>True si le mot de passe est valide, sinon False</returns>
        private bool ValidatePassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=(?:[^A-Z]*[A-Z]){2,})(?=(?:[^a-z]*[a-z]){2,})(?=(?:\D*\d){2,})(?=(?:[^\.\-\+#\[\]\*&<>\""]*[\.\-\+#\[\]\*&<>\""]){2,}).{12,}$");
        }

        /// <summary>
        /// Valide que la date de naissance est valide et que l'utilisateur a au moins 18 ans.
        /// </summary>
        /// <param name="birthDate">La date de naissance à valider</param>
        /// <returns>True si la date de naissance est valide, sinon False</returns>
        private bool ValidateBirthDate(DateTime birthDate)
        {
            return birthDate <= DateTime.Now && birthDate <= DateTime.Now.AddYears(-18);
        }

        /// <summary>
        /// Valide que l'adresse contient uniquement des lettres, des chiffres et des espaces et respecte la longueur maximale.
        /// </summary>
        /// <param name="address">L'adresse à valider</param>
        /// <returns>True si l'adresse est valide, sinon False</returns>
        private bool ValidateAddress(string address)
        {
            return Regex.IsMatch(address, @"^[a-zA-Z0-9\s]{1,80}$");
        }

        /// <summary>
        /// Gère la navigation vers la fenêtre de connexion.
        /// Ouvre la fenêtre de connexion et ferme la fenêtre actuelle.
        /// </summary>
        /// <param name="sender">Le contrôle qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        public void NavigateToConnexion(object sender, RoutedEventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow();
            connectionWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Gère la navigation vers la fenêtre principale.
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
