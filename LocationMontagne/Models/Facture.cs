using System;
namespace LocationMontagne.Models
{
    /// <summary>
    /// Classe de l'objet Facture
    /// </summary>
    public class Facture
    {
        private int _idFacture;
        public int idFacture 
        {
            get => _idFacture;
            set => _idFacture = value;
        }
        private DateTime _dateFacture;
        public DateTime dateFacture 
        {
            get => _dateFacture;
            set => _dateFacture = value;
        }
        private decimal _montant;
        public decimal montant 
        { 
            get => _montant; 
            set => _montant = value;
        }

        public Facture(int idFacture, DateTime dateFacture, decimal montant)
        {
            this.idFacture = idFacture;
            this.dateFacture = dateFacture;
            this.montant = montant;
        }
    }

}
