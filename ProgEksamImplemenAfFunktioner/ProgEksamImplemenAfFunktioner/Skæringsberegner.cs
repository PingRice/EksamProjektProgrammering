using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.RootFinding;

namespace ProgEksamImplemenAfFunktioner
{
    internal class Skæringsberegner
    {
        public static (double? x, double? y) FindIntersection(Func<double, double> f1, Func<double, double> f2, double start, double end)
        {
            // Definer en ny funktion, der repræsenterer forskellen mellem de to funktioner
            Func<double, double> h = x =>
            {
                if (x >= start && x <= end)
                {
                    return f1(x) - f2(x);
                }
                else
                {
                    return double.NaN;
                }
            };

            try
            {
                // Find skæringspunktet ved hjælp af MathNet
                double x_skæring = Bisection.FindRoot(h, start, end);

                if (double.IsNaN(x_skæring))
                {
                    return (null, null);
                }
                else
                {
                    double y_skæring = f1(x_skæring);
                    return (x_skæring, y_skæring);
                }
            }
            catch (MathNet.Numerics.NonConvergenceException)
            {
                return (null, null);
            }
        }
    }
}
