using System;
namespace LocationMontagne.Models
{
    /// <summary>
    /// Classe de l'objet User
    /// </summary>
    public class User
    {
        private int _id;
        public int id
        {
            get => _id;
            set => _id = value;
        }
        private string _nom;
        public string nom
        {
            get => _nom;
            set => _nom = value;
        }
        private string _prenom;
        public string prenom
        {
            get => _prenom;
            set => _prenom = value;
        }
        private string _email;
        public string email
        {
            get => _email; 
            set => _email = value;
        }
        private string _login;
        public string login
        {
            get => _login;
            set => _login = value;
        }
        private string _password;
        public string password
        {
            get => _password; 
            set => _password = value;
        }
        private DateTime _dateNaissance;
        public DateTime dateNaissance
        {
            get => _dateNaissance; 
            set => _dateNaissance = value;
        }
        private string _adresse;
        public string adresse
        {
            get => _adresse;
            set => _adresse = value;
        }
        private bool _estEmploye;
        public bool estEmploye
        {
            get => _estEmploye;
            set => _estEmploye = value;
        }
        private Ville _ville;
        public Ville ville
        {
            get => _ville; 
            set => _ville = value;
        }

        public User(int id, string nom, string prenom, string email, string login, string password, DateTime dateNaissance, string adresse, bool estEmploye, Ville ville)
        {
            this.id = id;
            this.nom = nom;
            this.prenom = prenom;
            this.email = email;
            this.login = login;
            this.password = password;
            this.dateNaissance = dateNaissance;
            this.adresse = adresse;
            this.estEmploye = estEmploye;
            this.ville = ville;
        }

        public User()
        {

        }
    }
}
