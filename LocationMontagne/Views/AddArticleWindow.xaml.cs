using LocationMontagne.Models;
using LocationMontagne.ViewModels;
using Microsoft.Win32;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace LocationMontagne.Views
{
    /// <summary>
    /// Logique d'interaction pour AddArticleWindow.xaml
    /// </summary>
    public partial class AddArticleWindow : Window
    {
        private readonly ArticleVM articleVM;
        private readonly CategorieVM categorieVM;
        private string selectedImagePath;

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre d'ajout d'article
        /// avec les ViewModels nécessaires.
        /// </summary>
        /// <param name="articleVM">Le ViewModel des articles</param>
        /// <param name="categorieVM">Le ViewModel des catégories</param>
        public AddArticleWindow(ArticleVM articleVM, CategorieVM categorieVM)
        {
            InitializeComponent();
            this.articleVM = articleVM;
            this.categorieVM = categorieVM;
            InitializeCategories();
        }

        /// <summary>
        /// Initialise le ComboBox des catégories avec toutes les catégories disponibles,
        /// en ajoutant une option par défaut "Choisissez une catégorie" au début.
        /// </summary>
        private void InitializeCategories()
        {
            CategorieComboBox.Items.Add(new Categorie(0, "Choisissez une catégorie"));
            foreach (var categorie in categorieVM.categories)
            {
                CategorieComboBox.Items.Add(categorie);
            }
            CategorieComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Gère l'entrée du curseur dans la zone de dépôt d'image avec un fichier.
        /// Change l'apparence visuelle de la bordure et définit l'effet de glisser-déposer.
        /// </summary>
        /// <param name="sender">La zone de dépôt qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de glisser-déposer</param>
        private void ImageDropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                ImageDropZone.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#2563EB");
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Gère la sortie du curseur de la zone de dépôt d'image.
        /// Restaure l'apparence normale de la bordure.
        /// </summary>
        /// <param name="sender">La zone de dépôt qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de glisser-déposer</param>
        private void ImageDropZone_DragLeave(object sender, DragEventArgs e)
        {
            ImageDropZone.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#D1D5DB");
        }

        /// <summary>
        /// Gère le dépôt d'une image dans la zone prévue à cet effet.
        /// Vérifie si le fichier déposé est une image valide et l'affiche en prévisualisation.
        /// </summary>
        /// <param name="sender">La zone de dépôt qui déclenche l'événement</param>
        /// <param name="e">Les données de l'image déposée</param>
        private void ImageDropZone_Drop(object sender, DragEventArgs e)
        {
            ImageDropZone.BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#D1D5DB");

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && IsImageFile(files[0]))
                {
                    selectedImagePath = files[0];
                    PreviewImage.Source = new BitmapImage(new Uri(selectedImagePath));
                }
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton "Parcourir" pour sélectionner une image depuis le système de fichiers.
        /// Affiche l'image sélectionnée en prévisualisation.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.gif|Tous les fichiers|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(selectedImagePath));
            }
        }

        /// <summary>
        /// Vérifie si un fichier est une image valide basée sur son extension.
        /// </summary>
        /// <param name="fileName">Le chemin complet du fichier à vérifier</param>
        /// <returns>True si le fichier est une image (jpg, jpeg, png, gif), sinon False</returns>
        private bool IsImageFile(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif";
        }

        /// <summary>
        /// Gère le clic sur le bouton "Ajouter Catégorie".
        /// Ouvre une fenêtre d'ajout de catégorie et met à jour le ComboBox si une nouvelle catégorie est créée.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void AjouterCategorie_Click(object sender, RoutedEventArgs e)
        {
            var addCategorieWindow = new AddCategorieWindow(categorieVM);
            bool? result = addCategorieWindow.ShowDialog();

            if (result == true && addCategorieWindow.NouvelleCategorie != null)
            {
                CategorieComboBox.SelectedItem = addCategorieWindow.NouvelleCategorie;
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton "Ajouter" pour créer un nouvel article.
        /// Valide les entrées utilisateur, crée un nouvel article et le sauvegarde dans la base de données.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void Ajouter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomArticleTextBox.Text))
            {
                MessageBox.Show("Veuillez saisir un nom d'article.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TarifTextBox.Text, out decimal tarif))
            {
                MessageBox.Show("Veuillez saisir un tarif valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(StockTextBox.Text, out int stock))
            {
                MessageBox.Show("Veuillez saisir une quantité de stock valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategorieComboBox.SelectedItem == null)
            {
                MessageBox.Show("Veuillez sélectionner une catégorie.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var nouvelArticle = new Article(
            NomArticleTextBox.Text,
            DescriptionTextBox.Text,
            tarif,
            stock,
            null,
            (Categorie)CategorieComboBox.SelectedItem
        );

            if (articleVM.AjouterArticle(nouvelArticle, selectedImagePath))
            {
                MessageBox.Show("Article ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton "Annuler".
        /// Ferme la fenêtre sans sauvegarder d'article.
        /// </summary>
        /// <param name="sender">Le bouton qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Contrôle la saisie dans le champ de tarif pour n'accepter que des chiffres et des séparateurs décimaux.
        /// </summary>
        /// <param name="sender">Le TextBox qui déclenche l'événement</param>
        /// <param name="e">Les données de composition du texte</param>
        private void TarifTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9,.]$");
        }

        /// <summary>
        /// Contrôle la saisie dans le champ de stock pour n'accepter que des chiffres.
        /// </summary>
        /// <param name="sender">Le TextBox qui déclenche l'événement</param>
        /// <param name="e">Les données de composition du texte</param>
        private void StockTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]$");
        }
    }
}
