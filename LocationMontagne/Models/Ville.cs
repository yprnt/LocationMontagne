namespace LocationMontagne.Models
{
    /// <summary>
    /// Classe de l'objet Ville
    /// </summary>
    public class Ville
    {
        private int _idVille;
        public int idVille 
        {
            get => _idVille;
            set => _idVille = value;
        }
        private string _nomVille;
        public string nomVille 
        {
            get => _nomVille;
            set => _nomVille = value;
        }
        private string _codePostal;
        public string codePostal 
        {
            get => _codePostal; 
            set => _codePostal = value;
        }

        public Ville(int idVille, string nomVille, string codePostal)
        {
            this.idVille = idVille;
            this.nomVille = nomVille;
            this.codePostal = codePostal;
        }
    }

}
