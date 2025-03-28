using LocationMontagne.Models;
using LocationMontagne.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace LocationMontagne.Views
{
    /// <summary>
    /// Logique d'interaction pour EmployeeDashboardWindow.xaml
    /// </summary>
    public partial class EmployeeDashboardWindow : Window
    {
        private readonly ArticleVM articleVM;
        private readonly CategorieVM categorieVM;
        private readonly Dictionary<Article, string> tempImagePaths;
        private Article articleEnEdition = null;

        /// <summary>
        /// Initialise une nouvelle instance du dashboard employé
        /// avec les informations liées aux articles et aux catégories.
        /// </summary>
        public EmployeeDashboardWindow()
        {
            InitializeComponent();
            this.categorieVM = new CategorieVM();
            this.articleVM = new ArticleVM();
            this.tempImagePaths = new Dictionary<Article, string>();
            DataContext = articleVM;
            InitializeCategories();
            InitializeCategorieColumn();
            ArticlesDataGrid.Loaded += (s, e) => UpdateButtonsState();
        }

        /// <summary>
        /// Gère le glisser-déposer d'une image sur la zone de dépôt d'un article.
        /// </summary>
        /// <param name="sender">La border qui déclenche l'évènement</param>
        /// <param name="e">Donnée de l'image déposée</param>
        private void ImageDropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && IsImageFile(files[0]))
                {
                    var imageElement = (Image)((Grid)((Border)sender).Child).Children[0];
                    var article = (Article)((FrameworkElement)sender).DataContext;

                    if (article != null)
                    {
                        string newPath = files[0]; 
                        tempImagePaths[article] = newPath; 
                        imageElement.Source = new BitmapImage(new Uri(newPath));
                    }
                }
            }
        }

        /// <summary>
        /// Gère le clic sur le bouton "Parcourir" pour sélectionner une image depuis le système de fichiers.
        /// </summary>
        /// <param name="sender">Le boutton qui déclenche le browse</param>
        /// <param name="e">Les données de l'image déposée</param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.gif|Tous les fichiers|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var imageElement = (Image)((Grid)((Button)sender).Parent).Children[0];
                var article = (Article)((FrameworkElement)sender).DataContext;

                if (article != null)
                {
                    string newPath = openFileDialog.FileName;
                    tempImagePaths[article] = newPath; 
                    imageElement.Source = new BitmapImage(new Uri(newPath));
                }
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
        /// Initialise le ComboBox des catégories avec toutes les catégories disponibles,
        /// en ajoutant une option "Toutes les catégories" au début.
        /// </summary>
        private void InitializeCategories()
        {
            CategorieComboBox.Items.Add(new Categorie(0, "Toutes les catégories"));
            foreach (var categorie in categorieVM.categories)
            {
                CategorieComboBox.Items.Add(categorie);
            }
            CategorieComboBox.SelectedIndex = 0;
        }


        /// <summary>
        /// Configure la colonne des catégories dans le DataGrid pour utiliser les données
        /// du ViewModel des catégories.
        /// </summary>
        private void InitializeCategorieColumn()
        {
            var categorieColumn = ArticlesDataGrid.Columns
                .FirstOrDefault(c => c.Header.ToString() == "Catégorie") as DataGridComboBoxColumn;

            if (categorieColumn != null)
            {
                var categoriesForColumn = new ObservableCollection<Categorie>(categorieVM.categories);
                categorieColumn.ItemsSource = categoriesForColumn;
                categorieColumn.SelectedItemBinding = new Binding("categorie");
                categorieColumn.DisplayMemberPath = "nomCategorie";
                categorieColumn.SelectedValuePath = "idCategorie";
            }
        }

        /// <summary>
        /// Gère le changement de sélection dans le ComboBox des catégories pour filtrer 
        /// les articles affichés dans le DataGrid.
        /// </summary>
        /// <param name="sender">Le combobox à l'origine du déclenchement</param>
        /// <param name="e">La catégorie séléctionné par l'utilisateur</param>
        private void CategorieComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategorieComboBox.SelectedValue != null)
            {
                int idCategorie = (int)CategorieComboBox.SelectedValue;
                if (idCategorie == 0)
                {
                    ArticlesDataGrid.ItemsSource = articleVM.articles;
                }
                else
                {
                    ArticlesDataGrid.ItemsSource = articleVM.GetArticlesByCategorie(idCategorie);
                }
            }
            else
            {
                ArticlesDataGrid.ItemsSource = articleVM.articles;
            }

            articleEnEdition = null;
            Dispatcher.BeginInvoke(new Action(() => UpdateButtonsState()), 
                System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// Gère le clic sur le bouton "Ajouter Article" pour ouvrir la fenêtre d'ajout d'article
        /// et actualise l'état des boutons après l'ajout.
        /// </summary>
        /// <param name="sender">Le bouton ajouter un article</param>
        /// <param name="e">Rien, aucune donnée n'est utilisée</param>
        private void AjouterArticle_Click(object sender, RoutedEventArgs e)
        {
            var addArticleWindow = new AddArticleWindow(articleVM, categorieVM);
            addArticleWindow.ShowDialog();
            Dispatcher.BeginInvoke(new Action(() => UpdateButtonsState()),
                System.Windows.Threading.DispatcherPriority.Background);
        }


        /// <summary>
        /// Gère la fin de l'édition d'une cellule dans le DataGrid.
        /// Réinitialise l'état d'édition et valide les modifications de l'article.
        /// </summary>
        /// <param name="sender">Le DataGrid</param>
        /// <param name="e">Donnée de l'éditions</param>
        private void ArticlesDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                articleEnEdition = null;
                UpdateButtonsState();

                foreach (var item in ArticlesDataGrid.Items)
                {
                    DataGridRow row = ArticlesDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    if (row != null)
                    {
                        row.IsEnabled = true;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Render);

            if (e.Row.Item is Article articleModifie)
            {
                if (articleModifie.categorie == null)
                {
                    MessageBox.Show("Veuillez sélectionner une catégorie valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }

                if (!articleVM.UpdateArticle(articleModifie))
                {
                    e.Cancel = true;
                }
            }
        }


        /// <summary>
        /// Gère le clic sur le bouton "Supprimer" pour un article.
        /// Affiche une confirmation et supprime l'article de la base de données si confirmé.
        /// </summary>
        /// <param name="sender">Le boutton supprimé</param>
        /// <param name="e">Les données lié à la ligne du DataGrid</param>
        private void SupprimerArticle_Click(object sender, RoutedEventArgs e)
        {
            var article = ((FrameworkElement)sender).DataContext as Article;
            if (article != null)
            {
                var result = MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer l'article '{article.nomArticle}' ?",
                    "Confirmation de suppression",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    articleVM.DeleteArticle(article.idArticle);
                    Dispatcher.BeginInvoke(new Action(() => UpdateButtonsState()),
                System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }


        /// <summary>
        /// Gère le clic sur le bouton "Modifier" pour un article.
        /// Affiche une confirmation et enregistre les modifications si confirmé.
        /// </summary>
        /// <param name="sender">Le bouton modifier</param>
        /// <param name="e">Les données de la ligne du DataGrid</param>
        private void ModifierArticle_Click(object sender, RoutedEventArgs e)
        {
            var article = ((FrameworkElement)sender).DataContext as Article;
            if (article != null)
            {
                var result = MessageBox.Show(
                    $"Voulez-vous enregistrer les modifications de l'article '{article.nomArticle}' ?",
                    "Confirmation de modification",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (article.categorie == null)
                    {
                        MessageBox.Show("Veuillez sélectionner une catégorie valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string newImagePath = null;
                    if (tempImagePaths.TryGetValue(article, out string path))
                    {
                        newImagePath = path;
                        tempImagePaths.Remove(article);
                    }

                    if (articleVM.UpdateArticle(article, newImagePath))
                    {
                        MessageBox.Show("Article modifié avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }


        /// <summary>
        /// Gère le début de l'édition d'une cellule dans le DataGrid.
        /// Empêche l'édition simultanée de plusieurs articles et désactive les lignes non éditées.
        /// </summary>
        /// <param name="sender">Le datagrid</param>
        /// <param name="e">Données pour le début de l'édition</param>
        private void ArticlesDataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (articleEnEdition != null && articleEnEdition != e.Row.Item)
            {
                e.Cancel = true;
                return;
            }

            articleEnEdition = e.Row.Item as Article;
            UpdateButtonsState();

            foreach (var item in ArticlesDataGrid.Items)
            {
                if (item != articleEnEdition)
                {
                    DataGridRow row = ArticlesDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    if (row != null)
                    {
                        row.IsEnabled = false;
                    }
                }
            }
        }


        /// <summary>
        /// Gère la préparation d'une cellule pour l'édition.
        /// Définit l'article en cours d'édition et met à jour l'état des boutons.
        /// </summary>
        /// <param name="sender">Le DataGrid</param>
        /// <param name="e">Les données de l'article (de base)</param>
        private void ArticlesDataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            articleEnEdition = e.Row.Item as Article;
            UpdateButtonsState();
            ArticlesDataGrid.IsReadOnly = false;
        }

        /// <summary>
        /// Gère la fin de l'édition d'une ligne entière dans le DataGrid.
        /// Réinitialise l'état d'édition et des boutons après la validation ou l'annulation des modifications.
        /// </summary>
        /// <param name="sender">Le DataGrid</param>
        /// <param name="e">Donnée de l'édition de la ligne du DataGrid</param>
        private void ArticlesDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit || e.EditAction == DataGridEditAction.Cancel)
            {
                Dispatcher.BeginInvoke(new Action(() => //Met à jour l'UI de manière asyncrhone
                {
                    articleEnEdition = null;
                    UpdateButtonsState();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }


        /// <summary>
        /// Met à jour l'état des boutons "Modifier" dans le DataGrid en fonction de l'article en cours d'édition.
        /// Désactive également le bouton "Ajouter Article" pendant une édition.
        /// </summary>
        private void UpdateButtonsState()
        {
            ArticlesDataGrid.UpdateLayout();

            foreach (var item in ArticlesDataGrid.Items)
            {
                if (ArticlesDataGrid.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) // vérifie si tous les contenneurs sont générés
                {
                    var row = ArticlesDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    if (row == null)
                    {
                        ArticlesDataGrid.ScrollIntoView(item);
                        ArticlesDataGrid.UpdateLayout();
                        row = ArticlesDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    }

                    if (row != null)
                    {
                        var modifierButton = FindModifierButton(row);
                        if (modifierButton != null)
                        {
                            bool shouldBeEnabled = (articleEnEdition == null || articleEnEdition == item);
                            if (modifierButton.IsEnabled != shouldBeEnabled)
                            {
                                modifierButton.IsEnabled = shouldBeEnabled;
                            }
                        }
                    }
                }
            }
            var addButton = this.FindName("AjouterArticle") as Button;
            if (addButton != null)
            {
                addButton.IsEnabled = (articleEnEdition == null);
            }
        }


        /// <summary>
        /// Trouve le bouton "Modifier" dans une ligne spécifique du DataGrid en parcourant l'arbre visuel.
        /// </summary>
        /// <param name="row">La ligne du DataGrid = DataGridRow</param>
        /// <returns>Le bouton "Modifier", sinon null</returns>
        private Button FindModifierButton(DataGridRow row)
        {
            var presenter = VisualTreeHelper.GetChild(row, 0) as DataGridCellsPresenter;
            if (presenter != null)
            {
                var cell = presenter.ItemContainerGenerator.ContainerFromIndex(ArticlesDataGrid.Columns.Count - 1) as DataGridCell;
                if (cell != null)
                {
                    var panel = VisualTreeHelper.GetChild(cell, 0) as StackPanel;
                    if (panel != null && panel.Children.Count > 0)
                    {
                        return panel.Children[0] as Button;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gère le clic sur le bouton "Déconnexion".
        /// Affiche une confirmation et, si confirmé, ferme la fenêtre actuelle et retourne au MainWindow.
        /// </summary>
        /// <param name="sender">Le boutton déconnexion</param>
        /// <param name="e">Aucune donnée</param>
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

        /// <summary>
        /// Gère le clic sur l'onglet "Locations".
        /// Ouvre la fenêtre de gestion des locations.
        /// </summary>
        /// <param name="sender">Le border qui déclenche l'évènement</param>
        /// <param name="e">Les données du MouseLeftButtonDown</param>
        private void LocationsNav_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var locationsWindow = new EmployeeLocationsWindow();
            locationsWindow.Show();
            this.Close();
        }
    }
}
