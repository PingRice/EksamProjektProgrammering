using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;


namespace ProgEksamImplemenAfFunktioner
{
    public partial class Form1 : Form
    {
        // Laserstråle
        Func<double, double> f = x => -4 * x + 3;

        // Liste over objekter (funktioner og deres intervaller)
        private readonly List<(Func<double, double> funktion, double start, double end, string navn)> funktioner =
    new List<(Func<double, double> funktion, double start, double end, string navn)>
    {
        // TIlføj flere funktioner her
        //Første reflekterede
        (x => x >= 0.37 && x <= 1.24 ? 0.39 * x + 1.3 : double.NaN, 0.37, 1.24, "j"),
        //Anden reflekterede 
        (x => x >= 0.8 && x <= 1.6 ? -0.27 * x + 0.54 : double.NaN , 0.8, 1.6, "p"),
        //Tredje reflekterede 
        (x => x >= 3.4 && x <= 3.52 ? 3.06*x-8.87 : double.NaN, 3.4, 3.52, "k"),
        //Fjerde reflekterede
        (x => x >= 2.75 && x <= 3.44 ? -0.53*x+4.5 : double.NaN, 2.75, 3.44, "l"),
        //Femte reflekterede
        (x => x >= -0.85 && x <= -0.23 ? 0.36 * x + 2.41 : double.NaN, -0.83, -0.23, "d")

    };

        public Form1()
        {
            InitializeComponent();
            BeregnSkæringer();
        }
        private void BeregnSkæringer()
        {
            // Startpunkt for laser
            double xStart = 0.75;
            double yStart = 0;

            // Startfunktion for laser
            Func<double, double> laserStart = x => -4 * x + 3;

            StartLaserReflections(laserStart, xStart, yStart);
        }

        private void StartLaserReflections(Func<double, double> startFunction, double xStart, double yStart)
        {
            Func<double, double> currentFunction = startFunction;
            double currentX = xStart;
            double currentY = yStart;

            int refleksionNr = 1;
            // Før loopet, find første retningsvektor
            (double dx, double dy) = BeregnRetningsvektor(startFunction, xStart);

            while (true)
            {
                var (closestFunction, x, y) = FindClosestIntersection(currentFunction, currentX, currentY, dx, dy);

                if (!x.HasValue || !y.HasValue)
                {
                    Console.WriteLine($"Refleksion #{refleksionNr}: Ingen flere skæringer – stop.");
                    break;
                }

                Console.WriteLine($"Refleksion #{refleksionNr}: Skæringspunkt ({x.Value}, {y.Value})");
                //Der laves et objekt af klassen reflektionsberegner.
                var refleksion = new Reflektionsberegner(currentX, currentY, currentFunction, closestFunction, x.Value, y.Value);
                refleksion.CalculateReflection();

                currentFunction = refleksion.GetReflectedFunction();
                currentX = x.Value;
                currentY = y.Value;

                // Beregn ny retningsvektor efter refleksion
                //refleksion.GetReflectedAngle() returnerer den udgående vinkel vOut (i radianer, da du ganger med Math.PI / 180).
                //Math.Cos giver x-komponenten (hvor meget laseren bevæger sig vandret)
                //Math.Sin giver y-komponenten (hvor meget den bevæger sig lodret) baseret på den vinkel.
                (dx, dy) = (Math.Cos(refleksion.GetReflectedAngle() * Math.PI / 180), Math.Sin(refleksion.GetReflectedAngle() * Math.PI / 180));

                refleksionNr++;
            }
        }
        //Metoden finder ud fra de fundne skæringspunkter, den funktion der ligger tættest på forrige skæringspunkt. 
        private (Func<double, double>, double?, double?) FindClosestIntersection(Func<double, double> laserFunc, double xFrom, double yFrom, double dx, double dy)
        {
            double minDistance = double.MaxValue;
            Func<double, double> closestFunc = null;
            double? closestX = null;
            double? closestY = null;

            foreach (var (funktion, start, end, navn) in funktioner)
            {
                var skæring = Skæringsberegner.FindIntersection(laserFunc, funktion, start, end);
                if (skæring.x.HasValue && skæring.y.HasValue)
                {
                    double x = skæring.x.Value;
                    double y = skæring.y.Value;

                    // Et tjek der specifikt tjekker om der er skæring med funktionen "l" samt beregner afstand og dotprodukt mellem de to vektorer (hvor meget de peger i samme retning). 
                    if (navn == "l")
                    {
                        Console.WriteLine($"POTENTIEL SKÆRING MED 'l' FUNDET VED (x: {x}, y: {y})");
                        Console.WriteLine($"  Afstand fra nuværende position: {Math.Sqrt(Math.Pow(x - xFrom, 2) + Math.Pow(y - yFrom, 2))}");
                        Console.WriteLine($"  Dotprodukt med retningsvektor: {(x - xFrom) * dx + (y - yFrom) * dy}");
                    }

                    double diffX = x - xFrom;
                    double diffY = y - yFrom;

                    // Dotprodukt: hvis > 0 så er punktet i samme retning som vektoren
                    double dotProduct = diffX * dx + diffY * dy;

                   //Finder det nærmeste skæringspunkt, hvor afstanden skal være positiv. 
                    double distance = Math.Sqrt(diffX * diffX + diffY * diffY);
                    if (distance > 0.0001 && distance < minDistance)
                    {
                        minDistance = distance;
                        closestFunc = funktion;
                        closestX = x;
                        closestY = y;
                    }
                }
            }

            return (closestFunc, closestX, closestY);
        }
        //Metoden beregner retningsvektoren, hvor der vælges en lille forskydning i x-retningen, som bruges til at finde et punkt lidt fremme i funktionen. 
        //Retningen afhænger af fortegnet på deltaX, hvor hvis den er positiv vil den bevæge sig fremad i x-retningen, og modsat hvis den er negativ. Ex -0.01 / 0.01. 
        private (double dx, double dy) BeregnRetningsvektor(Func<double, double> funktion, double xStart, double deltaX = -0.01)
        {
            double x2 = xStart + deltaX;
            double y1 = funktion(xStart);
            double y2 = funktion(x2);

            double dx = x2 - xStart;
            double dy = y2 - y1;
            
            return (dx, dy);
        }

    }
}
