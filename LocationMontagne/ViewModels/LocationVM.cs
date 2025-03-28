using LocationMontagne.Models;
using LocationMontagne.Services;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LocationMontagne.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des locations.
    /// Méthodes pour créer, récupérer, modifier, annuler et marquer des locations comme retournées.
    /// </summary>
    public class LocationVM
    {
        private readonly EtatLocationVM _etatLocationVM;

        /// <summary>
        /// Initialise une nouvelle instance de la classe LocationVM.
        /// Crée une instance de EtatLocationVM pour accéder aux états de location.
        /// </summary>
        public LocationVM()
        {
            _etatLocationVM = new EtatLocationVM();
        }

        /// <summary>
        /// Crée une nouvelle location avec facturation automatique.
        /// Utilise une transaction pour assurer l'intégrité des données lors de la création
        /// de la facture, de la location, de l'association article-location et de la mise à jour du stock.
        /// </summary>
        /// <param name="dateDebut">La date de début de la location</param>
        /// <param name="dateFin">La date de fin de la location</param>
        /// <param name="quantite">La quantité d'articles à louer</param>
        /// <param name="user">L'utilisateur qui effectue la location</param>
        /// <param name="article">L'article à louer</param>
        /// <returns>L'objet Location créé si la création a réussi, sinon null</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion, de requête ou de transaction</exception>
        public Location CreateLocation(DateTime dateDebut, DateTime dateFin, int quantite, User user, Article article)
        {
            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string insertFactureQuery = @"
                        INSERT INTO facture (dateFacture, montant)
                        VALUES (@DateFacture, @Montant);
                        SELECT LAST_INSERT_ID();";

                            int idFacture;
                            decimal montantTotal = article.tarif * quantite;
                            using (MySqlCommand cmd = new MySqlCommand(insertFactureQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@DateFacture", DateTime.Now);
                                cmd.Parameters.AddWithValue("@Montant", montantTotal);
                                idFacture = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            Facture facture = new Facture(idFacture, DateTime.Now, montantTotal);

                            string insertLocationQuery = @"
                        INSERT INTO location (dateLocation, dateDebutLocation, dateFinLocation, idUser, idEtatLocation, idFacture)
                        VALUES (@DateLocation, @DateDebut, @DateFin, @IdUser, @IdEtat, @IdFacture);
                        SELECT LAST_INSERT_ID();";

                            int idLocation;
                            using (MySqlCommand cmd = new MySqlCommand(insertLocationQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@DateLocation", DateTime.Now);
                                cmd.Parameters.AddWithValue("@DateDebut", dateDebut);
                                cmd.Parameters.AddWithValue("@DateFin", dateFin);
                                cmd.Parameters.AddWithValue("@IdUser", user.id);
                                cmd.Parameters.AddWithValue("@IdEtat", _etatLocationVM.GetEtatLocationByNom("Payée").idEtatLocation);
                                cmd.Parameters.AddWithValue("@IdFacture", idFacture);

                                idLocation = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            string insertLocationArticleQuery = @"
                        INSERT INTO locationArticle (idLocation, idArticle, quantite)
                        VALUES (@IdLocation, @IdArticle, @Quantite)";

                            using (MySqlCommand cmd = new MySqlCommand(insertLocationArticleQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@IdLocation", idLocation);
                                cmd.Parameters.AddWithValue("@IdArticle", article.idArticle);
                                cmd.Parameters.AddWithValue("@Quantite", quantite);
                                cmd.ExecuteNonQuery();
                            }

                            string updateStockQuery = @"
                        UPDATE article 
                        SET quantiteStock = quantiteStock - @Quantite 
                        WHERE idArticle = @IdArticle";

                            using (MySqlCommand cmd = new MySqlCommand(updateStockQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Quantite", quantite);
                                cmd.Parameters.AddWithValue("@IdArticle", article.idArticle);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            return new Location(
                                idLocation,
                                DateTime.Now,
                                dateDebut,
                                dateFin,
                                DateTime.MinValue,
                                user,
                                _etatLocationVM.GetEtatLocationByNom("Payée"),
                                facture
                            );
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création de la location : {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Récupère toutes les locations d'un utilisateur spécifique.
        /// </summary>
        /// <param name="user">L'utilisateur dont on souhaite récupérer les locations</param>
        /// <returns>Une liste des locations de l'utilisateur, triées par date de location décroissante</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public List<Location> GetLocationsByUser(User user)
        {
            List<Location> locations = new List<Location>();

            string query = @"
                SELECT 
                    l.idLocation, l.dateLocation, l.dateDebutLocation, 
                    l.dateFinLocation, l.dateRetourArticle,
                    e.idEtatLocation, e.nomEtatLocation,
                    f.idFacture, f.dateFacture, f.montant
                FROM location l
                INNER JOIN etatlocation e ON l.idEtatLocation = e.idEtatLocation
                INNER JOIN facture f ON l.idFacture = f.idFacture
                WHERE l.idUser = @IdUser
                ORDER BY l.dateLocation DESC";

            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdUser", user.id);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Facture facture = null;
                                if (!reader.IsDBNull(reader.GetOrdinal("idFacture")))
                                {
                                    facture = new Facture(
                                        reader.GetInt32("idFacture"),
                                        reader.GetDateTime("dateFacture"),
                                        reader.GetDecimal("montant")
                                    );
                                }

                                EtatLocation etatLocation = new EtatLocation(
                                    reader.GetInt32("idEtatLocation"),
                                    reader.GetString("nomEtatLocation")
                                );

                                DateTime dateRetourArticle = DateTime.MinValue;
                                if (!reader.IsDBNull(reader.GetOrdinal("dateRetourArticle")))
                                {
                                    dateRetourArticle = reader.GetDateTime("dateRetourArticle");
                                }

                                Location location = new Location(
                                    reader.GetInt32("idLocation"),
                                    reader.GetDateTime("dateLocation"),
                                    reader.GetDateTime("dateDebutLocation"),
                                    reader.GetDateTime("dateFinLocation"),
                                    dateRetourArticle,
                                    user,
                                    etatLocation,
                                    facture
                                );

                                locations.Add(location);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération des réservations : {ex.Message}");
            }

            return locations;
        }

        /// <summary>
        /// Récupère tous les articles associés à une location spécifique.
        /// </summary>
        /// <param name="location">La location dont on souhaite récupérer les articles</param>
        /// <returns>Une liste des articles de la location avec leurs quantités</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public List<LocationArticle> GetArticlesForLocation(Location location)
        {
            List<LocationArticle> locationArticles = new List<LocationArticle>();

            string query = @"
                SELECT 
                    la.quantite,
                    a.idArticle, a.nomArticle, a.description, a.tarif, 
                    a.quantiteStock, a.image,
                    c.idCategorie, c.nomCategorie
                FROM locationarticle la
                INNER JOIN article a ON la.idArticle = a.idArticle
                INNER JOIN categorie c ON a.idCategorie = c.idCategorie
                WHERE la.idLocation = @IdLocation";

            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdLocation", location.idLocation);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Categorie categorie = new Categorie(
                                    reader.GetInt32("idCategorie"),
                                    reader.GetString("nomCategorie")
                                );

                                Article article = new Article(
                                    reader.GetInt32("idArticle"),
                                    reader.GetString("nomArticle"),
                                    reader.GetString("description"),
                                    reader.GetDecimal("tarif"),
                                    reader.GetInt32("quantiteStock"),
                                    reader.GetString("image"),
                                    categorie
                                );

                                LocationArticle locationArticle = new LocationArticle(
                                    location,
                                    article,
                                    reader.GetInt32("quantite")
                                );

                                locationArticles.Add(locationArticle);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération des articles : {ex.Message}");
            }

            return locationArticles;
        }

        /// <summary>
        /// Annule une location et remet les articles en stock.
        /// Utilise une transaction pour assurer l'intégrité des données.
        /// </summary>
        /// <param name="location">La location à annuler</param>
        /// <returns>True si l'annulation a réussi, sinon False</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion, de requête ou de transaction</exception>
        public bool CancelLocation(Location location)
        {
            if (location.etatLocation.nomEtatLocation != "Payée" &&
                location.etatLocation.nomEtatLocation != "En attente de paiement")
            {
                MessageBox.Show("Impossible d'annuler cette réservation car elle est déjà " +
                               location.etatLocation.nomEtatLocation.ToLower() + ".");
                return false;
            }

            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string updateLocationQuery = @"
                        UPDATE location 
                        SET idEtatLocation = 5
                        WHERE idLocation = @IdLocation
                        AND idEtatLocation IN (1, 2)";

                            using (MySqlCommand cmd = new MySqlCommand(updateLocationQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@IdLocation", location.idLocation);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected == 0)
                                {
                                    throw new Exception("La réservation ne peut pas être annulée.");
                                }
                            }

                            string updateStockQuery = @"
                        UPDATE article a
                        INNER JOIN locationarticle la ON a.idArticle = la.idArticle
                        SET a.quantiteStock = a.quantiteStock + la.quantite
                        WHERE la.idLocation = @IdLocation";

                            using (MySqlCommand cmd = new MySqlCommand(updateStockQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@IdLocation", location.idLocation);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("Erreur lors de l'annulation : " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Met à jour les dates d'une location existante.
        /// Vérifie d'abord que la location est dans un état permettant la modification.
        /// </summary>
        /// <param name="location">La location à modifier</param>
        /// <param name="newDateDebut">La nouvelle date de début</param>
        /// <param name="newDateFin">La nouvelle date de fin</param>
        /// <returns>True si la mise à jour a réussi, sinon False</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public bool UpdateLocation(Location location, DateTime newDateDebut, DateTime newDateFin)
        {
            if (location.etatLocation.nomEtatLocation != "Payée" &&
                location.etatLocation.nomEtatLocation != "En attente de paiement")
            {
                MessageBox.Show("Impossible de modifier cette réservation car elle est " +
                               location.etatLocation.nomEtatLocation.ToLower() + ".");
                return false;
            }

            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    string updateQuery = @"
                UPDATE location 
                SET dateDebutLocation = @DateDebut,
                    dateFinLocation = @DateFin
                WHERE idLocation = @IdLocation
                AND idEtatLocation IN (1, 2)"; 

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateDebut", newDateDebut);
                        cmd.Parameters.AddWithValue("@DateFin", newDateFin);
                        cmd.Parameters.AddWithValue("@IdLocation", location.idLocation);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification de la réservation : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Marque une location comme retournée en mettant à jour son état et la date de retour.
        /// Vérifie d'abord que la location est dans un état permettant le retour.
        /// </summary>
        /// <param name="location">La location à marquer comme retournée</param>
        /// <returns>True si la mise à jour a réussi, sinon False</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public bool MarquerLocationRetournee(Location location)
        {
            if (location.etatLocation.nomEtatLocation != "Payée" &&
                location.etatLocation.nomEtatLocation != "En cours")
            {
                MessageBox.Show("Impossible de marquer cette réservation comme retournée car elle est " +
                               location.etatLocation.nomEtatLocation.ToLower() + ".");
                return false;
            }

            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    string updateQuery = @"
                UPDATE location 
                SET idEtatLocation = 4,
                    dateRetourArticle = @DateRetour
                WHERE idLocation = @IdLocation
                AND (idEtatLocation = 2 OR idEtatLocation = 3)";

                    using (MySqlCommand cmd = new MySqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@DateRetour", DateTime.Now);
                        cmd.Parameters.AddWithValue("@IdLocation", location.idLocation);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du marquage de la réservation comme retournée : {ex.Message}");
                return false;
            }
        }
    }
}
