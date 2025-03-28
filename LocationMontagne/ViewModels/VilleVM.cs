using LocationMontagne.Models;
using LocationMontagne.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using MySqlConnector;

namespace LocationMontagne.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des villes.
    /// Méthodes pour charger et ajouter des villes.
    /// </summary>
    public class VilleVM
    {
        private ObservableCollection<Ville> _villes;
        public ObservableCollection<Ville> villes
        {
            get => _villes;
            set => _villes = value;
        }

        /// <summary>
        /// Initialise une nouvelle instance de la classe VilleVM.
        /// Crée une collection vide de villes et charge les données depuis la base de données.
        /// </summary>
        public VilleVM()
        {
            villes = new ObservableCollection<Ville>();
            LoadVilles();
        }

        /// <summary>
        /// Charge toutes les villes depuis la base de données et les ajoute à la collection observable.
        /// </summary>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        private void LoadVilles()
        {
            try
            {
                using (MySqlConnection connection = Database.getConnection())
                {
                    string query = "SELECT idVille, nomVille, codePostal FROM ville ORDER BY nomVille";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            villes.Add(new Ville(
                                reader.GetInt32("idVille"),
                                reader.GetString("nomVille"),
                                reader.GetString("codePostal")
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des villes : " + ex.Message);
            }
        }

        /// <summary>
        /// Ajoute une nouvelle ville à la base de données et à l'observable collection.
        /// Vérifie d'abord si la ville existe déjà avec le même nom et code postal.
        /// Valide également le format du code postal.
        /// </summary>
        /// <param name="nomVille">Le nom de la ville à ajouter</param>
        /// <param name="codePostal">Le code postal de la ville (doit être composé de 5 chiffres)</param>
        /// <returns>L'identifiant de la ville (existante ou nouvelle) si l'opération réussit, sinon -1</returns>
        /// <exception cref="ArgumentException">Levée si le nom de la ville ou le code postal est invalide</exception>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public int AjouterVille(string nomVille, string codePostal)
        {
            if (string.IsNullOrWhiteSpace(nomVille) || string.IsNullOrWhiteSpace(codePostal))
                throw new ArgumentException("Le nom de la ville et le code postal sont requis.");

            if (!Regex.IsMatch(codePostal, @"^\d{5}$"))
                throw new ArgumentException("Le code postal doit contenir exactement 5 chiffres.");

            nomVille = nomVille.Trim();

            try
            {
                using (MySqlConnection connection = Database.getConnection())
                {
                    string checkQuery = "SELECT idVille FROM ville WHERE LOWER(nomVille) = LOWER(@nomVille) AND codePostal = @codePostal";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@nomVille", nomVille);
                    checkCmd.Parameters.AddWithValue("@codePostal", codePostal);

                    var existingId = checkCmd.ExecuteScalar();
                    if (existingId != null)
                    {
                        MessageBox.Show($"La ville {nomVille} ({codePostal}) existe déjà dans la base de données.",
                                      "Ville existante", MessageBoxButton.OK, MessageBoxImage.Information);
                        return Convert.ToInt32(existingId);
                    }

                    string insertQuery = "INSERT INTO ville (nomVille, codePostal) VALUES (@nomVille, @codePostal); SELECT LAST_INSERT_ID();";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@nomVille", nomVille);
                    insertCmd.Parameters.AddWithValue("@codePostal", codePostal);

                    int newId = Convert.ToInt32(insertCmd.ExecuteScalar());

                    Ville nouvelleVille = new Ville(newId, nomVille, codePostal);
                    villes.Add(nouvelleVille);

                    return newId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'ajout de la ville : " + ex.Message,
                               "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }
    }
}
