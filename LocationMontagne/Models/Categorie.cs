namespace LocationMontagne.Models
{
    /// <summary>
    /// Classe de l'objet Categorie
    /// </summary>
    public class Categorie
    {
        private int _idCategorie;
        public int idCategorie 
        {
            get => _idCategorie;
            set => _idCategorie = value;
        }
        private string _nomCategorie;
        public string nomCategorie 
        {
            get => _nomCategorie;
            set => _nomCategorie = value;
        }

        public Categorie(int idCategorie, string nomCategorie)
        {
            this.idCategorie = idCategorie;
            this.nomCategorie = nomCategorie;
        }
    }

}
