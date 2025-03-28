using LocationMontagne.Models;
using LocationMontagne.Services;
using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace LocationMontagne.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des articles de location.
    /// Méthodes pour charger, filtrer, ajouter, mettre à jour et supprimer des articles.
    /// </summary>
    public class ArticleVM
    {
        /// <summary>
        /// Observable Collection contenant tous les articles disponibles.
        /// </summary>
        private ObservableCollection<Article> _articles;

        /// <summary>
        /// Obtient ou définit l'Observable Collection des articles.
        /// Utilisée pour la liaison de données avec l'interface utilisateur.
        /// </summary>
        public ObservableCollection<Article> articles
        {
            get => _articles;
            set => _articles = value;
        }


        /// <summary>
        /// Initialise une nouvelle instance de la classe ArticleVM.
        /// Crée une collection vide d'articles et charge les données depuis la base de données.
        /// </summary>
        public ArticleVM()
        {
            articles = new ObservableCollection<Article>();

            LoadArticles();
        }


        /// <summary>
        /// Charge tous les articles depuis la base de données et les ajoute à la collection observable.
        /// Associe chaque article à sa catégorie correspondante.
        /// </summary>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public void LoadArticles()
        {
            try
            {
                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = @"
                SELECT a.idArticle, a.nomArticle, a.description, a.tarif, a.quantiteStock, a.image,
                       c.idCategorie, c.nomCategorie 
                FROM article a
                INNER JOIN categorie c ON a.idCategorie = c.idCategorie
                ORDER BY a.nomArticle";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categorie categorie = new Categorie(
                                reader.GetInt32("idCategorie"),
                                reader.GetString("nomCategorie")
                            );

                            articles.Add(new Article(
                                reader.GetInt32("idArticle"),
                                reader.GetString("nomArticle"),
                                reader.GetString("description"),
                                reader.GetDecimal("tarif"),
                                reader.GetInt32("quantiteStock"),
                                reader.IsDBNull(reader.GetOrdinal("image")) ? null : reader.GetString("image"),
                                categorie
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des articles : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Ajoute un nouvel article à la base de données et à l'observable collection.
        /// Vérifie d'abord si un article avec le même nom n'existe pas déjà dans la même catégorie.
        /// Gère également le traitement de l'image de l'article si fournie.
        /// </summary>
        /// <param name="nouvelArticle">L'objet Article contenant les informations du nouvel article</param>
        /// <param name="imagePath">Le chemin vers l'image de l'article (optionnel)</param>
        /// <returns>True si l'ajout a réussi, sinon False</returns>
        public bool AjouterArticle(Article nouvelArticle, string imagePath = null)
        {
            try
            {
                if (ArticleExists(nouvelArticle.nomArticle, nouvelArticle.categorie.idCategorie))
                {
                    MessageBox.Show($"Un article nommé '{nouvelArticle.nomArticle}' existe déjà dans la catégorie '{nouvelArticle.categorie.nomCategorie}'.",
                                  "Article déjà existant", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                string fileName = null;
                if (!string.IsNullOrEmpty(imagePath))
                {
                    fileName = ImageService.SaveImage(imagePath);
                }

                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = @"
                    INSERT INTO article (nomArticle, description, tarif, quantiteStock, image, idCategorie) 
                    VALUES (@nomArticle, @description, @tarif, @quantiteStock, @image, @idCategorie);
                    SELECT LAST_INSERT_ID();";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nomArticle", nouvelArticle.nomArticle);
                    command.Parameters.AddWithValue("@description", nouvelArticle.description);
                    command.Parameters.AddWithValue("@tarif", nouvelArticle.tarif);
                    command.Parameters.AddWithValue("@quantiteStock", nouvelArticle.quantiteStock);
                    command.Parameters.AddWithValue("@image", fileName);
                    command.Parameters.AddWithValue("@idCategorie", nouvelArticle.categorie.idCategorie);

                    int newId = Convert.ToInt32(command.ExecuteScalar());
                    nouvelArticle.idArticle = newId;
                    nouvelArticle.image = fileName;

                    articles.Add(nouvelArticle);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout de l'article : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Récupère tous les articles appartenant à une catégorie spécifique.
        /// </summary>
        /// <param name="idCategorie">L'identifiant de la catégorie à filtrer</param>
        /// <returns>Une nouvelle observable collection contenant uniquement les articles de la catégorie spécifiée</returns>
        public ObservableCollection<Article> GetArticlesByCategorie(int idCategorie)
        {
            var articlesFiltres = new ObservableCollection<Article>();
            foreach (var article in articles)
            {
                if (article.categorie.idCategorie == idCategorie)
                {
                    articlesFiltres.Add(article);
                }
            }
            return articlesFiltres;
        }


        /// <summary>
        /// Met à jour les informations d'un article dans la base de données.
        /// Vérifie d'abord qu'aucun autre article avec le même nom n'existe dans la même catégorie.
        /// Gère également la mise à jour de l'image si une nouvelle est fournie.
        /// </summary>
        /// <param name="article">L'objet Article contenant les informations mises à jour</param>
        /// <param name="newImagePath">Le chemin vers la nouvelle image (optionnel)</param>
        /// <returns>True si la mise à jour a réussi, sinon False</returns>
        public bool UpdateArticle(Article article, string newImagePath = null)
        {
            try
            {
                if (ArticleExists(article.nomArticle, article.categorie.idCategorie, article.idArticle))
                {
                    MessageBox.Show($"Un autre article nommé '{article.nomArticle}' existe déjà dans la catégorie '{article.categorie.nomCategorie}'.",
                                  "Article déjà existant", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                string fileName = article.image;
                if (!string.IsNullOrEmpty(newImagePath))
                {
                    if (!string.IsNullOrEmpty(article.image))
                    {
                        ImageService.DeleteImage(article.image);
                    }
                    fileName = ImageService.SaveImage(newImagePath);
                }

                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = @"
                UPDATE article 
                SET nomArticle = @nomArticle, 
                    description = @description, 
                    tarif = @tarif, 
                    quantiteStock = @quantiteStock, 
                    image = @image,
                    idCategorie = @idCategorie
                WHERE idArticle = @idArticle";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nomArticle", article.nomArticle);
                    command.Parameters.AddWithValue("@description", article.description);
                    command.Parameters.AddWithValue("@tarif", article.tarif);
                    command.Parameters.AddWithValue("@quantiteStock", article.quantiteStock);
                    command.Parameters.AddWithValue("@image", fileName);
                    command.Parameters.AddWithValue("@idCategorie", article.categorie.idCategorie);
                    command.Parameters.AddWithValue("@idArticle", article.idArticle);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        article.image = fileName;
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour de l'article : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        /// <summary>
        /// Supprime un article de la base de données et de l'observable collection.
        /// Vérifie d'abord si l'article n'est pas utilisé dans des locations existantes.
        /// Supprime également l'image associée à l'article si elle existe.
        /// </summary>
        /// <param name="idArticle">L'identifiant de l'article à supprimer</param>
        /// <returns>True si la suppression a réussi, sinon False</returns>
        public bool DeleteArticle(int idArticle)
        {
            try
            {
                if (IsArticleInUse(idArticle))
                {
                    MessageBox.Show(
                        "Impossible de supprimer cet article car il est ou a été en location.",
                        "Suppression impossible",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }

                using (MySqlConnection connection = Database.getConnection())
                {
                    var articleToDelete = articles.FirstOrDefault(a => a.idArticle == idArticle);
                    if (articleToDelete != null && !string.IsNullOrEmpty(articleToDelete.image))
                    {
                        ImageService.DeleteImage(articleToDelete.image);
                    }

                    string query = "DELETE FROM article WHERE idArticle = @idArticle";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@idArticle", idArticle);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        articles.Remove(articleToDelete);
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression de l'article : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        /// <summary>
        /// Vérifie si un article avec le même nom existe déjà dans la même catégorie.
        /// Peut exclure un article spécifique de la vérification (utile lors des mises à jour).
        /// </summary>
        /// <param name="nomArticle">Le nom de l'article à vérifier</param>
        /// <param name="idCategorie">L'identifiant de la catégorie</param>
        /// <param name="excludeArticleId">L'identifiant de l'article à exclure de la vérification (optionnel)</param>
        /// <returns>True si un article avec le même nom existe déjà dans la catégorie, sinon False</returns>
        public bool ArticleExists(string nomArticle, int idCategorie, int? excludeArticleId = null)
        {
            try
            {
                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = "SELECT COUNT(idArticle) FROM article WHERE nomArticle = @nomArticle AND idCategorie = @idCategorie";
                    if (excludeArticleId.HasValue)
                    {
                        query += " AND idArticle != @excludeArticleId";
                    }

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nomArticle", nomArticle);
                    command.Parameters.AddWithValue("@idCategorie", idCategorie);
                    if (excludeArticleId.HasValue)
                    {
                        command.Parameters.AddWithValue("@excludeArticleId", excludeArticleId.Value);
                    }

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la vérification de l'article : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        /// <summary>
        /// Vérifie si un article est référencé dans une ou plusieurs locations.
        /// Cette vérification est nécessaire avant de supprimer un article pour maintenir l'intégrité des données.
        /// </summary>
        /// <param name="idArticle">L'identifiant de l'article à vérifier</param>
        /// <returns>True si l'article est utilisé dans au moins une location, sinon False</returns>
        private bool IsArticleInUse(int idArticle)
        {
            try
            {
                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = @"
                SELECT COUNT(idLocation) 
                FROM locationarticle 
                WHERE idArticle = @idArticle";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@idArticle", idArticle);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la vérification de l'utilisation de l'article : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
        }
    }
}