using System;
namespace LocationMontagne.Models
{
    /// <summary>
    /// Classe de l'objet Location
    /// </summary>
    public class Location
    {
        private int _idLocation;
        public int idLocation 
        {
            get => _idLocation;
            set => _idLocation = value;
        }
        private DateTime _dateLocation;
        public DateTime dateLocation 
        {
            get => _dateLocation;
            set => _dateLocation = value;
        }
        private DateTime _dateDebutLocation;
        public DateTime dateDebutLocation 
        {
            get => _dateDebutLocation; 
            set => _dateDebutLocation = value;
        }
        private DateTime _dateFinLocation;
        public DateTime dateFinLocation 
        {
            get => _dateFinLocation; 
            set => _dateFinLocation = value; 
        }
        private DateTime _dateRetourArticle;
        public DateTime dateRetourArticle 
        {
            get => _dateRetourArticle;
            set => _dateRetourArticle = value;
        }
        private User _user;
        public User user 
        {
            get => _user;
            set => _user = value;
        }
        private EtatLocation _etatLocation;
        public EtatLocation etatLocation 
        {
            get => _etatLocation;
            set => _etatLocation = value;
        }
        private Facture _facture;
        public Facture facture 
        {
            get => _facture; 
            set => _facture = value; 
        }

        public Location(int idLocation, DateTime dateLocation, DateTime dateDebutLocation, DateTime dateFinLocation, DateTime dateRetourArticle, User user, EtatLocation etatLocation, Facture facture)
        {
            this.idLocation = idLocation;
            this.dateLocation = dateLocation;
            this.dateDebutLocation = dateDebutLocation;
            this.dateFinLocation = dateFinLocation;
            this.dateRetourArticle = dateRetourArticle;
            this.user = user;
            this.etatLocation = etatLocation;
            this.facture = facture;
        }
    }

}
