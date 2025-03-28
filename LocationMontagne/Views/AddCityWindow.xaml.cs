using LocationMontagne.Models;
using LocationMontagne.ViewModels;
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
    /// Logique d'interaction pour AddCityWindow.xaml
    /// </summary>
    public partial class AddCityWindow : Window
    {
        private readonly VilleVM villeVM;
        public Ville NouvelleVille { get; private set; }

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre d'ajout de ville
        /// avec la fenêtre propriétaire et le ViewModel nécessaire.
        /// </summary>
        /// <param name="owner">La fenêtre propriétaire de cette boîte de dialogue</param>
        /// <param name="villesVM">Le ViewModel des villes</param>
        public AddCityWindow(Window owner, VilleVM villesVM)
        {
            InitializeComponent();
            villeVM = villesVM;
        }

        /// <summary>
        /// Gère le clic sur le bouton "Ajouter" pour créer une nouvelle ville.
        /// Tente d'ajouter la ville à la base de données et affiche un message d'erreur en cas d'échec.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void AjouterVille_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idVille = villeVM.AjouterVille(NomVilleTextBox.Text, CodePostalTextBox.Text);
                if (idVille != -1)
                {
                    NouvelleVille = new Ville(idVille, NomVilleTextBox.Text, CodePostalTextBox.Text);
                    DialogResult = true;
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton "Annuler".
        /// Ferme la fenêtre sans sauvegarder de ville.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
