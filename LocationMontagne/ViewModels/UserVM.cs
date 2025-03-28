using LocationMontagne.Models;
using LocationMontagne.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using MySqlCommand = MySqlConnector.MySqlCommand;
using MySqlConnection = MySqlConnector.MySqlConnection;
using MySqlDataReader = MySqlConnector.MySqlDataReader;

namespace LocationMontagne.ViewModels
{
    /// <summary>
    /// ViewModel pour la gestion des utilisateurs.
    /// Méthodes pour la connexion, l'inscription et la récupération des utilisateurs.
    /// </summary>
    public class UserVM
    {
        /// <summary>
        /// Authentifie un utilisateur en vérifiant ses identifiants.
        /// Vérifie le mot de passe hashé stocké en base de données avec BCrypt.
        /// </summary>
        /// <param name="user">L'objet User contenant le login et le mot de passe à vérifier</param>
        /// <returns>Un objet User complet si l'authentification réussit, sinon un User vide</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public User connexion(User user)
        {
            string getHashQuery = "SELECT password FROM user WHERE login = @login";
            string hashedPassword = null;

            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(getHashQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", user.login);
                        hashedPassword = cmd.ExecuteScalar()?.ToString();
                    }

                    if (hashedPassword != null && BCrypt.Net.BCrypt.Verify(user.password, hashedPassword))
                    {
                        string getUserQuery = @"SELECT u.idUser, u.nom, u.prenom, u.email, u.login, 
                                         u.password, u.dateNaiss, u.adresse, u.estEmploye, 
                                         v.idVille, v.nomVille, v.codePostal 
                                         FROM user u 
                                         INNER JOIN ville v ON u.idVille = v.idVille
                                         WHERE u.login = @login";

                        using (MySqlCommand cmd = new MySqlCommand(getUserQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@login", user.login);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return new User(
                                        reader.GetInt32("idUser"),
                                        reader.GetString("nom"),
                                        reader.GetString("prenom"),
                                        reader.GetString("email"),
                                        reader.GetString("login"),
                                        hashedPassword,
                                        reader.GetDateTime("dateNaiss"),
                                        reader.GetString("adresse"),
                                        reader.GetBoolean("estEmploye"),
                                        new Ville(
                                            reader.GetInt32("idVille"),
                                            reader.GetString("nomVille"),
                                            reader.GetString("codePostal")
                                        )
                                    );
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new User();
            }

            return new User(); 
        }

        /// <summary>
        /// Inscrit un nouvel utilisateur dans la base de données après vérification de l'unicité du login.
        /// Hash le mot de passe avant de le stocker en base de données.
        /// </summary>
        /// <param name="user">L'objet User contenant les informations du nouvel utilisateur</param>
        /// <param name="idVille">L'identifiant de la ville de l'utilisateur</param>
        /// <returns>True si l'inscription a réussi, sinon False</returns>
        /// <exception cref="Exception">Peut lever une exception si le login existe déjà ou en cas d'erreur de connexion</exception>
        public bool inscription(User user, int idVille)
        {
            string checkLoginQuery = "SELECT COUNT(idUser) FROM user WHERE login = @login";
            bool success = false;

            try
            {
                using (MySqlConnection connection = Database.getConnection())
                {
                    using (MySqlCommand checkCmd = new MySqlCommand(checkLoginQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@login", user.login);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            throw new Exception("Cet identifiant est déjà utilisé.");
                        }
                    }

                    string hashedPassword = HashPassword(user.password);

                    string insertQuery = @"INSERT INTO user (nom, prenom, email, login, password, datenaiss, adresse, idVille)
                                     VALUES (@Nom, @Prenom, @Email, @Login, @Password, @DateNaiss, @Adresse, @IdVille)";

                    using (MySqlCommand cmd = new MySqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Nom", user.nom);
                        cmd.Parameters.AddWithValue("@Prenom", user.prenom);
                        cmd.Parameters.AddWithValue("@Email", user.email);
                        cmd.Parameters.AddWithValue("@Login", user.login);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@DateNaiss", user.dateNaissance);
                        cmd.Parameters.AddWithValue("@Adresse", user.adresse);
                        cmd.Parameters.AddWithValue("@IdVille", idVille);

                        cmd.ExecuteNonQuery();
                        success = true;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return success;
        }

        /// <summary>
        /// Hash un mot de passe en utilisant l'algorithme BCrypt avec un coût de 12.
        /// </summary>
        /// <param name="password">Le mot de passe en clair à hasher</param>
        /// <returns>Le mot de passe hashé</returns>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, 12);
            
        }

        /// <summary>
        /// Récupère tous les utilisateurs (non employés) de la base de données.
        /// </summary>
        /// <returns>Liste de tous les utilisateurs</returns>
        /// <exception cref="Exception">Peut lever une exception en cas d'erreur de connexion ou de requête</exception>
        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            string query = @"
        SELECT u.idUser, u.nom, u.prenom, u.email, u.login, u.dateNaiss, u.adresse, u.estEmploye,
               v.idVille, v.nomVille, v.codePostal
        FROM user u
        INNER JOIN ville v ON u.idVille = v.idVille
        WHERE u.estEmploye = 0";

            try
            {
                using (MySqlConnection conn = Database.getConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Ville ville = new Ville(
                                    reader.GetInt32("idVille"),
                                    reader.GetString("nomVille"),
                                    reader.GetString("codePostal")
                                );

                                User user = new User(
                                    reader.GetInt32("idUser"),
                                    reader.GetString("nom"),
                                    reader.GetString("prenom"),
                                    reader.GetString("email"),
                                    reader.GetString("login"),
                                    string.Empty,
                                    reader.GetDateTime("dateNaiss"),
                                    reader.GetString("adresse"),
                                    reader.GetBoolean("estEmploye"),
                                    ville
                                );

                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération des utilisateurs : {ex.Message}");
            }

            return users;
        }
    }
}
