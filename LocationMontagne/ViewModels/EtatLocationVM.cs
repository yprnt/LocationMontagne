using LocationMontagne.Models;
using LocationMontagne.Services;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LocationMontagne.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des états de location.
    /// Méthodes pour charger les états et rechercher un état par son nom.
    /// </summary>
    public class EtatLocationVM
    {
        public List<EtatLocation> etatsLocation {  get; private set; }

        /// <summary>
        /// Initialise une nouvelle instance de la classe EtatLocationVM.
        /// Crée une liste vide d'états de location et charge les données depuis la base de données.
        /// </summary>
        public EtatLocationVM() 
        {
            etatsLocation = new List<EtatLocation>();
            LoadEtatsLocation();
        }

        /// <summary>
        /// Charge tous les états de location depuis la base de données et les ajoute à la liste.
        /// </summary>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        private void LoadEtatsLocation()
        {
            string query = "SELECT idEtatLocation, nomEtatLocation FROM etatlocation";
            try
            {
                using (MySqlConnection conn = Database.getConnection())
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        etatsLocation.Add(new EtatLocation(
                            reader.GetInt32("idEtatLocation"),
                            reader.GetString("nomEtatLocation")
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des états de location : {ex.Message}");
            }
        }

        /// <summary>
        /// Recherche et récupère un état de location par son nom.
        /// </summary>
        /// <param name="nom">Le nom de l'état de location à rechercher</param>
        /// <returns>L'objet EtatLocation correspondant au nom spécifié, ou null si aucun état avec ce nom n'est trouvé</returns>
        public EtatLocation GetEtatLocationByNom(string nom)
        {
            return etatsLocation.Find(e => e.nomEtatLocation == nom);
        }
    }
}
