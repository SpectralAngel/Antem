using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antem.Services
{
    public class Financial
    {
        /// <summary>
        /// Permite calcular los intereses adeudados en el periodo de un mes
        /// </summary>
        /// <param name="interes"></param>
        /// <param name="capital"></param>
        /// <returns></returns>
        public static decimal InteresMensual(double interes, decimal capital)
        {
            return capital * (decimal)interes / 1200;
        }

        /// <summary>
        /// Calcula el pago que se deberá efectuar tomando en cuenta el interes
        /// compuesto
        /// </summary>
        /// <param name="interes"></param>
        /// <param name="capital"></param>
        /// <param name="pagos"></param>
        /// <returns></returns>
        public static decimal PagoMensual(double interes, decimal capital, int pagos)
        {
            var i = interes / 1200;
            return capital * (decimal)(i / (1 - Math.Pow(i + 1, -pagos)));
        }

        public static decimal CalcularMora(decimal capital, double interes, DateTime ultimo)
        {
            var tiempo = DateTime.Now - ultimo;
            return capital * (decimal)interes * tiempo.Days / 30;
        }
    }
}
