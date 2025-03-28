using LocationMontagne.Models;
using System;
using System.Collections.Generic;
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

namespace LocationMontagne.Views
{
    /// <summary>
    /// Logique d'interaction pour LocationDetailsWindow.xaml
    /// </summary>
    public partial class LocationDetailsWindow : Window
    {
        private readonly Location _location;
        private readonly List<LocationArticle> _locationArticles;

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre de détails de location
        /// avec les informations de location et d'articles spécifiées.
        /// </summary>
        /// <param name="location">La location dont les détails sont affichés</param>
        /// <param name="locationArticles">La liste des articles associés à cette location</param>
        public LocationDetailsWindow(Location location, List<LocationArticle> locationArticles)
        {
            InitializeComponent();
            _location = location;
            _locationArticles = locationArticles;

            LoadLocationDetails();
        }

        /// <summary>
        /// Charge les détails de la location et des articles
        /// </summary>
        private void LoadLocationDetails()
        {
            // Informations de base de la location
            LocationNumberText.Text = $"#{_location.idLocation}";
            ReservationDateText.Text = _location.dateLocation.ToString("dd/MM/yyyy");
            PeriodText.Text = $"Du {_location.dateDebutLocation.ToString("dd/MM/yyyy")} au {_location.dateFinLocation.ToString("dd/MM/yyyy")}";

            // Informations client
            ClientText.Text = $"{_location.user.prenom} {_location.user.nom}";

            // État
            EtatText.Text = _location.etatLocation.nomEtatLocation;
            EtatBorder.Background = GetStatusColor(_location.etatLocation.nomEtatLocation);

            // Total
            if (_location.facture != null)
            {
                TotalText.Text = $"{_location.facture.montant:C}";
            }
            else
            {
                TotalText.Text = "Non facturé";
            }

            // Articles
            ArticlesItemsControl.ItemsSource = _locationArticles;
        }

        /// <summary>
        /// Retourne la couleur correspondant à l'état de la location
        /// </summary>
        /// <param name="status">L'état de la location</param>
        /// <returns>La couleur correspondante</returns>
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
        /// Ferme la fenêtre de détails
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}