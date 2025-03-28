namespace LocationMontagne.Models
{
    /// <summary>
    /// Classe de l'objet Article
    /// </summary>
    public class Article
    {
        private int _idArticle;
        public int idArticle 
        {
            get => _idArticle;
            set => _idArticle = value;
        }
        private string _nomArticle;
        public string nomArticle 
        {
            get => _nomArticle;
            set => _nomArticle = value;
        }
        private string _description;
        public string description 
        {
            get => _description;
            set => _description = value;
        }
        private decimal _tarif;
        public decimal tarif 
        {
            get => _tarif;
            set => _tarif = value;
        }
        private int _quantiteStock;
        public int quantiteStock 
        {
            get => _quantiteStock;
            set => _quantiteStock = value;
        }
        private string _image;
        public string image
        {
            get => _image;
            set => _image = value;
        }
        private Categorie _categorie;
        public Categorie categorie 
        {
            get => _categorie;
            set => _categorie = value;
        }

        public Article(int idArticle, string nomArticle, string description, decimal tarif, int quantiteStock, string image, Categorie categorie)
        {
            this.idArticle = idArticle;
            this.nomArticle = nomArticle;
            this.description = description;
            this.tarif = tarif;
            this.quantiteStock = quantiteStock;
            this.image = image;
            this.categorie = categorie;
        }

        public Article(string nomArticle, string description, decimal tarif, int quantiteStock, string image, Categorie categorie)
        {
            this.nomArticle = nomArticle;
            this.description = description;
            this.tarif = tarif;
            this.quantiteStock = quantiteStock;
            this.image = image;
            this.categorie = categorie;
        }
    }

}
