namespace LocationMontagne.Models
{
    /// <summary>
    /// Classe de l'objet EtatLocation
    /// </summary>
    public class EtatLocation
    {
        private int _idEtatLocation;
        public int idEtatLocation 
        {
            get => _idEtatLocation; 
            set => _idEtatLocation = value; 
        }
        private string _nomEtatLocation;
        public string nomEtatLocation 
        {
            get => _nomEtatLocation;
            set => _nomEtatLocation = value;
        }

        public EtatLocation(int idEtatLocation, string nomEtatLocation)
        {
            this.idEtatLocation = idEtatLocation;
            this.nomEtatLocation = nomEtatLocation;
        }
    }

}
