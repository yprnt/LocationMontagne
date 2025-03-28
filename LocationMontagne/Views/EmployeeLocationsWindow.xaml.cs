using LocationMontagne.Models;
using LocationMontagne.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LocationMontagne.Views
{
    /// <summary>
    /// Logique d'interaction pour EmployeeLocationsWindow.xaml
    /// </summary>
    public partial class EmployeeLocationsWindow : Window
    {
        private readonly LocationVM _locationVM;
        private List<Location> _allLocations;
        private string _selectedState;
        private readonly SolidColorBrush _activeButtonBrush;

        /// <summary>
        /// Initialise une nouvelle instance du dashboard employé
        /// avec la gestion des locations.
        /// </summary>
        public EmployeeLocationsWindow()
        {
            InitializeComponent();
            _locationVM = new LocationVM();
            _selectedState = "Tous les états";
            EtatComboBox.SelectedIndex = 0;

            _activeButtonBrush = (SolidColorBrush)FindResource("PrimaryColor");

            LoadLocations();
            UpdateFilterButtonsStyle(AllButton);
        }

        /// <summary>
        /// Charge toutes les locations depuis la base de données
        /// </summary>
        private void LoadLocations()
        {
            _allLocations = new List<Location>();
            List<User> users = new UserVM().GetAllUsers();

            foreach (var user in users)
            {
                var userLocations = _locationVM.GetLocationsByUser(user);
                _allLocations.AddRange(userLocations);
            }

            DisplayFilteredLocations();
        }

        /// <summary>
        /// Filtre les locations en fonction des critères sélectionnés et les affiche dans le DataGrid
        /// </summary>
        private void DisplayFilteredLocations()
        {
            if (_allLocations == null)
                return;

            var filteredLocations = _allLocations.Where(l =>
            {
                bool dateMatch = true;
                bool stateMatch = true;

                if (WeekButton.Background == _activeButtonBrush)
                    dateMatch = l.dateDebutLocation >= DateTime.Now.AddDays(-7);
                else if (MonthButton.Background == _activeButtonBrush)
                    dateMatch = l.dateDebutLocation >= DateTime.Now.AddDays(-30);

                if (_selectedState != "Tous les états")
                    stateMatch = l.etatLocation.nomEtatLocation == _selectedState;

                return dateMatch && stateMatch;
            }).OrderByDescending(l => l.dateLocation).ToList();

            LocationsDataGrid.ItemsSource = filteredLocations;
        }

        /// <summary>
        /// Gère le changement de sélection dans le ComboBox des états
        /// </summary>
        private void EtatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EtatComboBox.SelectedItem != null)
            {
                _selectedState = ((ComboBoxItem)EtatComboBox.SelectedItem).Content.ToString();
                DisplayFilteredLocations();
            }
        }

        /// <summary>
        /// Met à jour le style des boutons de filtre de date
        /// </summary>
        /// <param name="activeButton">Le bouton qui doit être actif</param>
        private void UpdateFilterButtonsStyle(Button activeButton)
        {
            WeekButton.Style = (Style)FindResource("FilterButtonStyle");
            MonthButton.Style = (Style)FindResource("FilterButtonStyle");
            AllButton.Style = (Style)FindResource("FilterButtonStyle");
            activeButton.Style = (Style)FindResource("ActiveFilterButtonStyle");
        }

        /// <summary>
        /// Filtre les locations des 7 derniers jours
        /// </summary>
        private void WeekFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilterButtonsStyle(WeekButton);
            DisplayFilteredLocations();
        }

        /// <summary>
        /// Filtre les locations des 30 derniers jours
        /// </summary>
        private void MonthFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilterButtonsStyle(MonthButton);
            DisplayFilteredLocations();
        }

        /// <summary>
        /// Affiche toutes les locations
        /// </summary>
        private void AllFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilterButtonsStyle(AllButton);
            DisplayFilteredLocations();
        }

        /// <summary>
        /// Marque une location comme retournée
        /// </summary>
        private void MarquerRetourne_Click(object sender, RoutedEventArgs e)
        {
            var location = ((Button)sender).DataContext as Location;
            if (location != null)
            {
                if (location.etatLocation.nomEtatLocation == "En cours" || location.etatLocation.nomEtatLocation == "Payée")
                {
                    var result = MessageBox.Show(
                        $"Confirmez-vous le retour des articles pour la location #{location.idLocation} ?",
                        "Confirmation de retour",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        if (_locationVM.MarquerLocationRetournee(location))
                        {
                            MessageBox.Show("Location marquée comme retournée avec succès.",
                                          "Succès",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);

                            LoadLocations();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Cette location ne peut pas être marquée comme retournée dans son état actuel.",
                                   "Information",
                                   MessageBoxButton.OK,
                                   MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Affiche les détails d'une location
        /// </summary>
        private void VoirDetails_Click(object sender, RoutedEventArgs e)
        {
            var location = ((Button)sender).DataContext as Location;
            if (location != null)
            {
                AfficherDetailsLocation(location);
            }
        }

        /// <summary>
        /// Gère le double-clic sur une ligne du DataGrid
        /// </summary>
        private void LocationsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var location = LocationsDataGrid.SelectedItem as Location;
            if (location != null)
            {
                AfficherDetailsLocation(location);
            }
        }

        /// <summary>
        /// Affiche les détails d'une location dans une nouvelle fenêtre
        /// </summary>
        /// <param name="location">La location à afficher</param>
        private void AfficherDetailsLocation(Location location)
        {
            var locationArticles = _locationVM.GetArticlesForLocation(location);
            var detailsWindow = new LocationDetailsWindow(location, locationArticles);
            detailsWindow.ShowDialog();
        }

        /// <summary>
        /// Revient à la gestion des articles
        /// </summary>
        private void ArticlesNav_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EmployeeDashboardWindow articleWindow = new EmployeeDashboardWindow();
            articleWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Se déconnecte et revient à la page de connexion
        /// </summary>
        private void Deconnexion_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir vous déconnecter ?",
                "Confirmation de déconnexion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }
    }
}