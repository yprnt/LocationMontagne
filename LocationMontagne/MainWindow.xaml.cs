using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LocationMontagne.Models;
using LocationMontagne.Services;
using LocationMontagne.ViewModels;
using LocationMontagne.Views;

namespace LocationMontagne
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CategorieVM categorieVM;
        private ArticleVM articleVM;
        private User currentUser;

        /// <summary>
        /// Initialise une nouvelle instance de la fenêtre principale
        /// avec l'utilisateur connecté (ou null pour un utilisateur invité).
        /// </summary>
        /// <param name="user">L'utilisateur connecté, ou null si mode invité</param>
        public MainWindow(User user = null)
        {
            InitializeComponent();
            categorieVM = new CategorieVM();
            articleVM = new ArticleVM();
            currentUser = user;
            InitializeCategories();
            UpdateUIForUser();
        }

        /// <summary>
        /// Met à jour l'interface utilisateur en fonction de l'état de connexion de l'utilisateur.
        /// Affiche soit les boutons d'authentification, soit les boutons utilisateur.
        /// </summary>
        private void UpdateUIForUser()
        {

            if (currentUser != null) 
            {
                AuthButtons.Visibility = Visibility.Collapsed;
                UserButtons.Visibility = Visibility.Visible;
            }
            else
            {
                AuthButtons.Visibility = Visibility.Visible;
                UserButtons.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gère la navigation vers la fenêtre des réservations de l'utilisateur.
        /// </summary>
        /// <param name="sender">Le contrôle qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void NavigateToReservations(object sender, RoutedEventArgs e)
        {
            ReservationWindow reservationWindow = new ReservationWindow(currentUser);
            reservationWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Gère la déconnexion de l'utilisateur.
        /// Réinitialise l'utilisateur courant et recharge la fenêtre principale en mode invité.
        /// </summary>
        /// <param name="sender">Le contrôle qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        private void Deconnexion(object sender, RoutedEventArgs e)
        {
            currentUser = null;
            MainWindow newWindow = new MainWindow();
            newWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Affiche une collection d'articles dans l'interface utilisateur.
        /// Crée une carte pour chaque article et les ajoute au panneau d'articles.
        /// </summary>
        /// <param name="articles">La collection d'articles à afficher</param>
        private void DisplayArticles(IEnumerable<Article> articles)
        {
            ArticlesWrapPanel.Children.Clear();

            foreach (var article in articles)
            {
                var card = CreateArticleCard(article);
                ArticlesWrapPanel.Children.Add(card);
            }
        }

        /// <summary>
        /// Crée une carte d'interface utilisateur pour un article spécifique.
        /// Inclut l'image, les informations et les boutons d'action.
        /// </summary>
        /// <param name="article">L'article pour lequel créer une carte</param>
        /// <returns>Un contrôle Border contenant la carte d'article</returns>
        private Border CreateArticleCard(Article article)
        {
            var card = new Border
            {
                Style = (Style)FindResource("ArticleCardStyle")
            };

            var mainStack = new StackPanel
            {
                Margin = new Thickness(0)
            };

            var imageContainer = new Border
            {
                Height = 180,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 16),
                ClipToBounds = true //image arrondis
            };

            // Image
            var image = new Image
            {
                Source = ImageService.LoadImage(article.image),
                Style = (Style)FindResource("ArticleImageStyle")
            };

            imageContainer.Child = image;

            // Grille pour image + badge dispo
            var imageGrid = new Grid();
            imageGrid.Children.Add(imageContainer);

            // Disponibilité
            var stockBadge = new Border
            {
                Style = (Style)FindResource("BadgeStyle"),
                Background = article.quantiteStock > 0
                    ? (SolidColorBrush)FindResource("SuccessColor")
                    : (SolidColorBrush)FindResource("DangerColor")
            };

            var badgeText = new TextBlock
            {
                Text = article.quantiteStock > 0 ? "Disponible" : "Rupture de stock",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };

            stockBadge.Child = badgeText;
            imageGrid.Children.Add(stockBadge);

            // Catégorie
            var categoryText = new TextBlock
            {
                Text = article.categorie.nomCategorie,
                Foreground = (SolidColorBrush)FindResource("SecondaryColor"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 8)
            };

            // Nom 
            var nomArticle = new TextBlock
            {
                Text = article.nomArticle,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = (SolidColorBrush)FindResource("TextPrimaryColor"),
                Margin = new Thickness(0, 0, 0, 8)
            };

            // Prix 
            var prix = new TextBlock
            {
                Text = $"{article.tarif:C}/jour",
                Style = (Style)FindResource("PriceStyle")
            };

            // Stock
            var stockInfo = new TextBlock
            {
                Text = $"En stock: {article.quantiteStock}",
                Foreground = article.quantiteStock > 0
                    ? (SolidColorBrush)FindResource("SuccessColor")
                    : (SolidColorBrush)FindResource("DangerColor"),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
            };

            // Boutons louer + détails
            var detailsButton = new Button
            {
                Content = "Détails",
                Style = (Style)FindResource("OutlineButtonStyle"),
                Width = 120,
                Height = 40,
                Margin = new Thickness(0, 0, 8, 0)
            };
            detailsButton.Click += (s, e) => ShowArticleDetails(article);

            var rentButton = new Button
            {
                Content = "Louer",
                Style = (Style)FindResource("PrimaryButtonStyle"),
                Width = 120,
                Height = 40
            };

            // Article indispo
            if (article.quantiteStock == 0)
            {
                rentButton.IsEnabled = false;
                rentButton.ToolTip = "Cet article n'est pas disponible actuellement";
            }
            else
            {
                rentButton.Click += (s, e) => {
                    if (currentUser != null)
                    {
                        AddReservationWindow reservationWindow = new AddReservationWindow(article, currentUser);
                        reservationWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        RedirectToConnection();
                    }
                };
            }

            // Boutons
            buttonsPanel.Children.Add(detailsButton);
            buttonsPanel.Children.Add(rentButton);

            // Assemblage des éléments
            mainStack.Children.Add(imageGrid);
            mainStack.Children.Add(categoryText);
            mainStack.Children.Add(nomArticle);
            mainStack.Children.Add(prix);
            mainStack.Children.Add(stockInfo);
            mainStack.Children.Add(buttonsPanel);

            card.Child = mainStack;

            return card;
        }

        /// <summary>
        /// Affiche une fenêtre modale avec les détails complets d'un article.
        /// Inclut l'image, la description, le prix, la disponibilité et les boutons d'action.
        /// </summary>
        /// <param name="article">L'article dont les détails sont à afficher</param>
        private void ShowArticleDetails(Article article)
        {
            var detailsWindow = new Window
            {
                Title = article.nomArticle,
                Width = 600,
                Height = 700,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Background = (SolidColorBrush)FindResource("BackgroundColor")
            };

            // Création d'un ScrollViewer pour une meilleure expérience
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0)
            };

            // StackPanel principal dans une Border avec effet d'ombre
            var border = new Border
            {
                Style = (Style)FindResource("CardStyle"),
                Margin = new Thickness(30)
            };

            var mainStack = new StackPanel
            {
                Margin = new Thickness(20)
            };

            // En-tête avec titre et badge de catégorie
            var headerPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 20)
            };

            var categoryBadge = new Border
            {
                Background = (SolidColorBrush)FindResource("SecondaryColor"),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var categoryText = new TextBlock
            {
                Text = article.categorie.nomCategorie,
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };
            categoryBadge.Child = categoryText;

            // Titre de l'article
            var nomArticle = new TextBlock
            {
                Text = article.nomArticle,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = (SolidColorBrush)FindResource("TextPrimaryColor"),
                Margin = new Thickness(0, 0, 0, 12)
            };

            headerPanel.Children.Add(categoryBadge);
            headerPanel.Children.Add(nomArticle);

            // Image de l'article
            var imageContainer = new Border
            {
                Height = 300,
                CornerRadius = new CornerRadius(8),
                ClipToBounds = true,
                Margin = new Thickness(0, 0, 0, 24)
            };

            var image = new Image
            {
                Source = ImageService.LoadImage(article.image),
                Stretch = Stretch.UniformToFill
            };
            imageContainer.Child = image;

            // Section prix et disponibilité
            var priceSectionBorder = new Border
            {
                Background = (SolidColorBrush)FindResource("BackgroundColor"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 24)
            };

            var priceSection = new StackPanel();

            var prix = new TextBlock
            {
                Text = $"Prix : {article.tarif:C} / jour",
                FontSize = 22,
                Foreground = (SolidColorBrush)FindResource("PrimaryColor"),
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var dispoBadge = new Border
            {
                Background = article.quantiteStock > 0
                    ? (SolidColorBrush)FindResource("SuccessColor")
                    : (SolidColorBrush)FindResource("DangerColor"),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var dispoText = new TextBlock
            {
                Text = article.quantiteStock > 0
                    ? $"En stock: {article.quantiteStock} disponible(s)"
                    : "Rupture de stock",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };
            dispoBadge.Child = dispoText;

            priceSection.Children.Add(prix);
            priceSection.Children.Add(dispoBadge);
            priceSectionBorder.Child = priceSection;

            // Section description
            var descriptionHeader = new TextBlock
            {
                Text = "Description",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            };

            var description = new TextBlock
            {
                Text = article.description,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 22,
                Margin = new Thickness(0, 0, 0, 24)
            };

            // Boutons d'action
            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 16, 0, 0)
            };

            var louerButton = new Button
            {
                Content = "Louer cet article",
                Style = (Style)FindResource("PrimaryButtonStyle"),
                Width = 200,
                Height = 44,
                Margin = new Thickness(0, 0, 16, 0)
            };

            if (article.quantiteStock == 0)
            {
                louerButton.IsEnabled = false;
                louerButton.ToolTip = "Cet article n'est pas disponible actuellement";
            }
            else
            {
                louerButton.Click += (s, e) => {
                    if (currentUser != null)
                    {
                        AddReservationWindow reservationWindow = new AddReservationWindow(article, currentUser);
                        reservationWindow.Show();
                        detailsWindow.Close();
                        this.Close();
                    }
                    else
                    {
                        RedirectToConnection();
                        detailsWindow.Close();
                    }
                };
            }

            var fermerButton = new Button
            {
                Content = "Fermer",
                Style = (Style)FindResource("OutlineButtonStyle"),
                Width = 120,
                Height = 44
            };
            fermerButton.Click += (s, e) => detailsWindow.Close();

            buttonsPanel.Children.Add(louerButton);
            buttonsPanel.Children.Add(fermerButton);

            // Assemblage des éléments
            mainStack.Children.Add(headerPanel);
            mainStack.Children.Add(imageContainer);
            mainStack.Children.Add(priceSectionBorder);
            mainStack.Children.Add(descriptionHeader);
            mainStack.Children.Add(description);
            mainStack.Children.Add(buttonsPanel);

            border.Child = mainStack;
            scrollViewer.Content = border;
            detailsWindow.Content = scrollViewer;
            detailsWindow.ShowDialog();
        }

        /// <summary>
        /// Redirige l'utilisateur vers la fenêtre de connexion.
        /// Utilisé lorsqu'une action nécessite une connexion.
        /// </summary>
        private void RedirectToConnection()
        {
            ConnectionWindow connectionWindow = new ConnectionWindow();
            connectionWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Gère le changement de catégorie dans le filtre.
        /// Affiche les articles de la catégorie sélectionnée.
        /// </summary>
        /// <param name="sender">Le ComboBox qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de changement de sélection</param>
        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedCategorie = CategoryFilter.SelectedItem as Categorie;
            if (selectedCategorie != null)
            {
                var articles = selectedCategorie.idCategorie == 0
                    ? articleVM.articles
                    : articleVM.GetArticlesByCategorie(selectedCategorie.idCategorie);
                DisplayArticles(articles);
            }
        }

        /// <summary>
        /// Initialise le ComboBox des catégories avec toutes les catégories disponibles,
        /// en ajoutant une option "Toutes les catégories" au début.
        /// </summary>
        private void InitializeCategories()
        {
            CategoryFilter.Items.Add(new Categorie(0, "Toutes les catégories"));
            foreach (var categorie in categorieVM.categories)
            {
                CategoryFilter.Items.Add(categorie);
            }
            CategoryFilter.SelectedIndex = 0;
        }

        /// <summary>
        /// Gère la navigation vers la fenêtre de connexion.
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
        /// Gère la navigation vers la fenêtre d'inscription.
        /// </summary>
        /// <param name="sender">Le contrôle qui déclenche l'événement</param>
        /// <param name="e">Les données associées à l'événement de clic</param>
        public void NavigateToInscription(object sender, RoutedEventArgs e)
        {
            InscriptionWindow inscriptionWindow = new InscriptionWindow();
            inscriptionWindow.Show();
            this.Close();
        }
    }
}
