using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Models
{
    /// <summary>
    /// Permite registrar los diversos movimientos de caja
    /// </summary>
    public class Invoice : Entity<int>
    {
        private Affiliate affiliate;
        private DateTime day;
        private IList<Charge> cargos = new List<Charge>();
        private bool isOutcome;

        public virtual Affiliate Affiliate
        {
            get { return affiliate; }
            set { affiliate = value; }
        }

        public virtual DateTime Fecha
        {
            get { return day; }
            set { day = value; }
        }

        public virtual IList<Charge> Cargos
        {
            get { return cargos; }
            set { cargos = value; }
        }

        public bool IsOutome
        {
            get { return isOutcome; }
            set { isOutcome = value; }
        }

        /// <summary>
        /// Calcula el total de la transacción registrada
        /// </summary>
        public decimal Total
        {
            get
            {
                return cargos.Sum(x => x.Amount);
            }
        }
    }
}
