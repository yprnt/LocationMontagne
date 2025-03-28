using LocationMontagne.Models;
using LocationMontagne.Services;
using MySqlConnector;
using System;
using System.Windows;

namespace LocationMontagne.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des factures.
    /// Méthodes pour récupérer et créer des factures.
    /// </summary>
    public class FactureVM
    {
        /// <summary>
        /// Récupère une facture depuis la base de données par son identifiant.
        /// </summary>
        /// <param name="idFacture">L'identifiant de la facture à récupérer</param>
        /// <returns>L'objet Facture correspondant à l'identifiant spécifié, ou null si aucune facture n'est trouvée</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public Facture GetFactureById(int idFacture)
        {
            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    string query = "SELECT idFacture, dateFacture, montant FROM facture WHERE idFacture = @idFacture";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@idFacture", idFacture);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Facture(
                                    reader.GetInt32("idFacture"),
                                    reader.GetDateTime("dateFacture"),
                                    reader.GetDecimal("montant")
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération de la facture : {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Crée une nouvelle facture et l'associe à une location.
        /// Utilise une transaction pour assurer l'intégrité des données.
        /// </summary>
        /// <param name="location">L'objet Location à associer à la facture</param>
        /// <param name="montant">Le montant de la facture</param>
        /// <returns>True si la création de la facture a réussi, sinon False</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion, de requête ou de transaction</exception>
        public bool CreateFacture(Location location, decimal montant)
        {
            try
            {
                using (MySqlConnection conn = Database.getConnection())
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string insertFactureQuery = @"
                            INSERT INTO facture (dateFacture, montant)
                            VALUES (@dateFacture, @montant);
                            SELECT LAST_INSERT_ID();";

                        int idFacture;
                        using (MySqlCommand cmd = new MySqlCommand(insertFactureQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@dateFacture", DateTime.Now);
                            cmd.Parameters.AddWithValue("@montant", montant);
                            idFacture = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        string updateLocationQuery = @"
                            UPDATE location 
                            SET idFacture = @idFacture
                            WHERE idLocation = @idLocation";

                        using (MySqlCommand cmd = new MySqlCommand(updateLocationQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idFacture", idFacture);
                            cmd.Parameters.AddWithValue("@idLocation", location.idLocation);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création de la facture : {ex.Message}");
                return false;
            }
        }
    }
}
