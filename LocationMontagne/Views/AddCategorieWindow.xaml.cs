using LocationMontagne.Models;
using LocationMontagne.ViewModels;
using System.Windows;


namespace LocationMontagne.Views
{
    /// <summary>
    /// Logique d'interaction pour AddCategorieWindow.xaml
    /// </summary>
    public partial class AddCategorieWindow : Window
    {
        private readonly CategorieVM categorieVM;
        public Categorie NouvelleCategorie { get; private set; }

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre d'ajout de catégorie
        /// avec le ViewModel nécessaire.
        /// </summary>
        /// <param name="categorieVM">Le ViewModel des catégories</param>
        public AddCategorieWindow(CategorieVM categorieVM)
        {
            InitializeComponent();
            this.categorieVM = categorieVM;
        }

        /// <summary>
        /// Gère le clic sur le bouton "Ajouter" pour créer une nouvelle catégorie.
        /// Valide l'entrée utilisateur, crée une nouvelle catégorie et la sauvegarde dans la base de données.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomCategorieTextBox.Text))
            {
                MessageBox.Show("Veuillez saisir un nom de catégorie.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NouvelleCategorie = categorieVM.AjouterCategorie(NomCategorieTextBox.Text);
            if (NouvelleCategorie != null)
            {
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton "Annuler".
        /// Ferme la fenêtre sans sauvegarder de catégorie.
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
