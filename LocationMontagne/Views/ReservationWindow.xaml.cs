using LocationMontagne.Models;
using LocationMontagne.Services;
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
    /// Logique d'interaction pour ReservationWindow.xaml
    /// </summary>
    public partial class ReservationWindow : Window
    {
        private readonly LocationVM _locationVM;
        private readonly User _currentUser;
        private List<Location> _allLocations;
        private DateTime _filterDate;
        private string _selectedState;
        private readonly SolidColorBrush _activeButtonBrush;
        private readonly SolidColorBrush _inactiveButtonBrush;

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre de réservations
        /// avec l'utilisateur connecté et configure les filtres par défaut.
        /// </summary>
        /// <param name="user">L'utilisateur connecté</param>
        public ReservationWindow(User user)
        {
            InitializeComponent();
            _locationVM = new LocationVM();
            _currentUser = user;
            _filterDate = DateTime.Now;
            _selectedState = "Tous les états";
            StateFilter.SelectedIndex = 0;

            _activeButtonBrush = (SolidColorBrush)FindResource("PrimaryColor");
            _inactiveButtonBrush = (SolidColorBrush)FindResource("BorderColor");

            LoadReservations();
            UpdateFilterButtonsStyle(AllButton);
        }

        /// <summary>
        /// Charge toutes les réservations de l'utilisateur connecté.
        /// </summary>
        private void LoadReservations()
        {
            _allLocations = _locationVM.GetLocationsByUser(_currentUser);
            DisplayFilteredReservations();
        }

        /// <summary>
        /// Affiche les réservations filtrées dans l'interface utilisateur.
        /// </summary>
        private void DisplayFilteredReservations()
        {
            ReservationsPanel.Children.Clear();
            var filteredLocations = FilterLocations();

            foreach (var location in filteredLocations)
            {
                var articles = _locationVM.GetArticlesForLocation(location);
                foreach (var locationArticle in articles)
                {
                    ReservationsPanel.Children.Add(CreateReservationCard(location, locationArticle));
                }
            }

            if (!filteredLocations.Any())
            {
                DisplayNoReservationsMessage();
            }
        }

        /// <summary>
        /// Filtre les réservations selon les critères de date et d'état sélectionnés.
        /// </summary>
        /// <returns>La liste des réservations filtrées</returns>
        private List<Location> FilterLocations()
        {
            if (_allLocations == null)
            {
                return new List<Location>();
            }

            return _allLocations.Where(l =>
            {
                bool dateMatch = true;
                bool stateMatch = true;

                if (WeekButton.Background == _activeButtonBrush)
                    dateMatch = l.dateDebutLocation >= _filterDate.AddDays(-7);
                else if (MonthButton.Background == _activeButtonBrush)
                    dateMatch = l.dateDebutLocation.Month == _filterDate.Month &&
                               l.dateDebutLocation.Year == _filterDate.Year;
                else if (YearButton.Background == _activeButtonBrush)
                    dateMatch = l.dateDebutLocation.Year == _filterDate.Year;

                if (_selectedState != "Tous les états")
                    stateMatch = l.etatLocation.nomEtatLocation == _selectedState;

                return dateMatch && stateMatch;
            }).OrderByDescending(l => l.dateDebutLocation).ToList();
        }

        /// <summary>
        /// Crée une carte de réservation avec les informations de la location et de l'article.
        /// </summary>
        /// <param name="location">La location à afficher</param>
        /// <param name="locationArticle">L'article associé à la location</param>
        /// <returns>Un contrôle Border contenant la carte de réservation</returns>
        private Border CreateReservationCard(Location location, LocationArticle locationArticle)
        {
            var card = new Border
            {
                Style = (Style)FindResource("ReservationCardStyle")
            };

            var mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });  // Image
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });   // Espacement
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });  // Informations
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });   // Espacement
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });  // Boutons

            var imageContainer = new Border
            {
                Width = 120,
                Height = 120,
                CornerRadius = new CornerRadius(8),
                ClipToBounds = true,
                Background = Brushes.White
            };

            // Image
            var image = new Image
            {
                Source = ImageService.LoadImage(locationArticle.article.image),
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };
            imageContainer.Child = image;
            Grid.SetColumn(imageContainer, 0);

            var infoPanel = new StackPanel { Margin = new Thickness(0) };
            Grid.SetColumn(infoPanel, 2);

            // État réservation
            var statusBadge = new Border
            {
                Background = GetStatusColor(location.etatLocation.nomEtatLocation),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 8)
            };
            var statusText = new TextBlock
            {
                Text = location.etatLocation.nomEtatLocation,
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            statusBadge.Child = statusText;

            // Nom
            var titleText = new TextBlock
            {
                Text = locationArticle.article.nomArticle,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource("PrimaryColor"),
                Margin = new Thickness(0, 0, 0, 8)
            };

            // Période 
            var periodText = new TextBlock
            {
                Text = $"Du {location.dateDebutLocation:dd/MM/yyyy} au {location.dateFinLocation:dd/MM/yyyy}",
                Margin = new Thickness(0, 0, 0, 8)
            };

            // Quantité
            var quantityText = new TextBlock
            {
                Text = $"Quantité : {locationArticle.quantite}",
                Margin = new Thickness(0, 0, 0, 8)
            };

            // Prix
            var priceText = new TextBlock
            {
                Text = $"Total : {location.facture?.montant:C}",
                FontWeight = FontWeights.SemiBold,
                Foreground = (SolidColorBrush)FindResource("SecondaryColor"),
                Margin = new Thickness(0, 8, 0, 0)
            };

            // Ajout des éléments
            infoPanel.Children.Add(statusBadge);
            infoPanel.Children.Add(titleText);
            infoPanel.Children.Add(periodText);
            infoPanel.Children.Add(quantityText);
            infoPanel.Children.Add(priceText);

            // Boutons panel
            var buttonsPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(buttonsPanel, 4);

            // Boutons
            var modifyButton = new Button
            {
                Content = "Modifier",
                Style = (Style)FindResource("PrimaryButtonStyle"),
                Width = 140,
                Height = 36,
                Margin = new Thickness(0, 0, 0, 8),
                IsEnabled = location.etatLocation.nomEtatLocation == "Payée" ||
                           location.etatLocation.nomEtatLocation == "En attente de paiement"
            };
            modifyButton.Click += (s, e) => ModifierLocation(location, locationArticle);

            var cancelButton = new Button
            {
                Content = "Annuler",
                Style = (Style)FindResource("DangerButtonStyle"),
                Width = 140,
                Height = 36,
                Margin = new Thickness(0, 0, 0, 8),
                IsEnabled = location.etatLocation.nomEtatLocation == "Payée" ||
                           location.etatLocation.nomEtatLocation == "En attente de paiement"
            };
            cancelButton.Click += (s, e) => AnnulerLocation(location);

            var invoiceButton = new Button
            {
                Content = "Voir la facture",
                Style = (Style)FindResource("OutlineButtonStyle"),
                Width = 140,
                Height = 36,
                IsEnabled = location.facture != null
            };
            invoiceButton.Click += (s, e) => VoirFacture(location, locationArticle);

            // Ajout des boutons
            buttonsPanel.Children.Add(modifyButton);
            buttonsPanel.Children.Add(cancelButton);
            buttonsPanel.Children.Add(invoiceButton);

            // Assemblage
            mainGrid.Children.Add(imageContainer);
            mainGrid.Children.Add(infoPanel);
            mainGrid.Children.Add(buttonsPanel);

            card.Child = mainGrid;

            return card;
        }

        /// <summary>
        /// Détermine la couleur correspondant à l'état de la location.
        /// </summary>
        /// <param name="status">L'état de la location sous forme de chaîne de caractères</param>
        /// <returns>La couleur de fond à utiliser pour représenter l'état</returns>
        private SolidColorBrush GetStatusColor(string status)
        {
            switch (status)
            {
                case "Payée":
                    return (SolidColorBrush)FindResource("SuccessColor");
                case "En cours":
                    return (SolidColorBrush)FindResource("InfoColor");
                case "Retournée":
                    return (SolidColorBrush)FindResource("SecondaryColor");
                case "Annulée":
                    return (SolidColorBrush)FindResource("DangerColor");
                case "En attente de paiement":
                    return (SolidColorBrush)FindResource("WarningColor");
                default:
                    return (SolidColorBrush)FindResource("TextSecondaryColor");
            }
        }

        /// <summary>
        /// Affiche un message lorsqu'aucune réservation ne correspond aux filtres sélectionnés.
        /// </summary>
        private void DisplayNoReservationsMessage()
        {
            var messageBlock = new TextBlock
            {
                Text = "Aucune réservation trouvée pour la période sélectionnée.",
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            ReservationsPanel.Children.Add(messageBlock);
        }

        /// <summary>
        /// Gère le changement de sélection dans le filtre d'état.
        /// Met à jour les réservations affichées selon l'état sélectionné.
        /// </summary>
        /// <param name="sender">Le ComboBox qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de changement de sélection</param>
        private void StateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StateFilter.SelectedItem != null)
            {
                _selectedState = ((ComboBoxItem)StateFilter.SelectedItem).Content.ToString();
                DisplayFilteredReservations();
            }
        }

        /// <summary>
        /// Met à jour le style des boutons de filtre de date en mettant en évidence le bouton actif.
        /// </summary>
        /// <param name="activeButton">Le bouton actif à mettre en évidence</param>
        private void UpdateFilterButtonsStyle(Button activeButton)
        {
            WeekButton.Background = _inactiveButtonBrush;
            MonthButton.Background = _inactiveButtonBrush;
            YearButton.Background = _inactiveButtonBrush;
            AllButton.Background = _inactiveButtonBrush;
            activeButton.Background = _activeButtonBrush;
        }

        /// <summary>
        /// Gère le clic sur le bouton de filtre "Semaine".
        /// Filtre les réservations pour ne montrer que celles de la semaine en cours.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void WeekFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilterButtonsStyle(WeekButton);
            DisplayFilteredReservations();
        }

        /// <summary>
        /// Gère le clic sur le bouton de filtre "Mois".
        /// Filtre les réservations pour ne montrer que celles du mois en cours.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void MonthFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilterButtonsStyle(MonthButton);
            DisplayFilteredReservations();
        }

        /// <summary>
        /// Gère le clic sur le bouton de filtre "Année".
        /// Filtre les réservations pour ne montrer que celles de l'année en cours.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void YearFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilterButtonsStyle(YearButton);
            DisplayFilteredReservations();
        }

        /// <summary>
        /// Gère le clic sur le bouton de filtre "Toutes".
        /// Affiche toutes les réservations sans filtre de date.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void AllFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateFilterButtonsStyle(AllButton);
            DisplayFilteredReservations();
        }

        /// <summary>
        /// Gère l'annulation d'une réservation.
        /// Affiche une confirmation et annule la réservation si confirmé.
        /// </summary>
        /// <param name="location">La location à annuler</param>
        private void AnnulerLocation(Location location)
        {
            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir annuler cette réservation ?",
                "Confirmation d'annulation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                if (_locationVM.CancelLocation(location))
                {
                    MessageBox.Show("La réservation a été annulée avec succès.",
                                  "Succès",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    LoadReservations();
                }
            }
        }

        /// <summary>
        /// Gère la modification d'une réservation.
        /// Ouvre la fenêtre de modification et recharge les réservations si des changements sont effectués.
        /// </summary>
        /// <param name="location">La location à modifier</param>
        /// <param name="locationArticle">L'article associé à la location</param>
        private void ModifierLocation(Location location, LocationArticle locationArticle)
        {
            var editWindow = new EditReservationWindow(location, locationArticle);
            if (editWindow.ShowDialog() == true)
            {
                LoadReservations();
            }
        }

        /// <summary>
        /// Gère l'affichage de la facture d'une réservation.
        /// Ouvre la fenêtre de facture pour la location et l'article spécifiés.
        /// </summary>
        /// <param name="location">La location dont la facture doit être affichée</param>
        /// <param name="locationArticle">L'article associé à la location</param>
        private void VoirFacture(Location location, LocationArticle locationArticle)
        {
            var factureWindow = new FactureWindow(location, locationArticle);
            factureWindow.ShowDialog();
        }

        /// <summary>
        /// Gère le clic sur le bouton "Retour".
        /// Retourne à la fenêtre principale en conservant l'utilisateur connecté.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(_currentUser);
            mainWindow.Show();
            this.Close();
        }
    }
}