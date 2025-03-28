namespace LocationMontagne.Models
{
    /// <summary>
    /// Classe de l'objet LocationArticle
    /// </summary>
    public class LocationArticle
    {
        private Location _location;
        public Location location 
        {
            get => _location;
            set => _location = value;
        }
        private Article _article;
        public Article article 
        {
            get => _article;
            set => _article = value;
        }
        private int _quantite;
        public int quantite 
        {
            get => _quantite;
            set => _quantite = value;
        }

        public LocationArticle(Location location, Article article, int quantite)
        {
            this.location = location;
            this.article = article;
            this.quantite = quantite;
        }
    }

}
