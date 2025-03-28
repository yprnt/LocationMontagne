using LocationMontagne.Models;
using LocationMontagne.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using MySqlConnector;

namespace LocationMontagne.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des catégories.
    /// Méthodes pour charger, ajouter et vérifier l'existence des catégories.
    /// </summary>
    public class CategorieVM
    {
        private ObservableCollection<Categorie> _categories;
        public ObservableCollection<Categorie> categories
        {
            get => _categories;
            set => _categories = value;
        }

        /// <summary>
        /// Initialise une nouvelle instance de la classe CategorieVM.
        /// Crée une collection vide de catégories et charge les données depuis la base de données.
        /// </summary>
        public CategorieVM()
        {
            categories = new ObservableCollection<Categorie>();
            LoadCategories();
        }

        /// <summary>
        /// Charge toutes les catégories depuis la base de données et les ajoute à la collection observable.
        /// </summary>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = "SELECT idCategorie, nomCategorie FROM categorie ORDER BY nomCategorie";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Categorie(
                                reader.GetInt32("idCategorie"),
                                reader.GetString("nomCategorie")
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des catégories : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Ajoute une nouvelle catégorie à la base de données et à l'observable collection.
        /// Vérifie d'abord si une catégorie avec le même nom n'existe pas déjà.
        /// </summary>
        /// <param name="nomCategorie">Le nom de la nouvelle catégorie à ajouter</param>
        /// <returns>L'objet Categorie créé si l'ajout a réussi, sinon null</returns>
        public Categorie AjouterCategorie(string nomCategorie)
        {
            try
            {
                if (CategorieExists(nomCategorie))
                {
                    MessageBox.Show($"Une catégorie nommée '{nomCategorie}' existe déjà.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = "INSERT INTO categorie (nomCategorie) VALUES (@nomCategorie); SELECT LAST_INSERT_ID();";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nomCategorie", nomCategorie);

                    int newId = Convert.ToInt32(command.ExecuteScalar());
                    var nouvelleCategorie = new Categorie(newId, nomCategorie);
                    categories.Add(nouvelleCategorie);
                    return nouvelleCategorie;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout de la catégorie : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Vérifie si une catégorie avec le même nom existe déjà dans la base de données.
        /// </summary>
        /// <param name="nomCategorie">Le nom de la catégorie à vérifier</param>
        /// <returns>True si une catégorie avec le même nom existe déjà, sinon False</returns>
        public bool CategorieExists(string nomCategorie)
        {
            try
            {
                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = "SELECT COUNT(nomCategorie) FROM categorie WHERE nomCategorie = @nomCategorie";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nomCategorie", nomCategorie);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la vérification de la catégorie : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
