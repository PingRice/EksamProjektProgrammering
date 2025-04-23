    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MathNet.Numerics;
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.RootFinding;

    namespace ProgEksamImplemenAfFunktioner
    {
        public class Reflektionsberegner
        {   
            //Laserstrålens x og y startpunkt. 
            public double XStart { get; set; }
            public double YStart { get; set; }
            
            //Definerer funktionen for laseren. 
            public Func<double, double> LaserFunction { get; set; }
            
            //Definerer funktion der beskriver den overfalde laseren rammer. 
            public Func<double, double> SurfaceFunction { get; set; }
            
            //x-værdien og y-værdien for af det punkt hvor laseren og overfladen skærer. 
            public double XIntersection { get; set; }
            public double YIntersection { get; set; }

            //Den udgående vinkel efter laserstrålen er reflekteret. 
            private double vOut;
            //Hældningen af den reflekterede laserstråle. 
            private double aOut;
            //Den reflekterede laserstråles skæringspunktet med y-aksen. 
            private double bOut;

            public Reflektionsberegner(double xStart, double yStart, Func<double, double> laserFunction, Func<double, double> surfaceFunction, double xIntersection, double yIntersection)
            {
                //De interne variabler defineres. 
                XStart = xStart;
                YStart = yStart;
                LaserFunction = laserFunction;
                SurfaceFunction = surfaceFunction;
                XIntersection = xIntersection;
                YIntersection = yIntersection;
            }
            //Metoden der beregner reflektionen af laseren. 
            public void CalculateReflection()
            {
                // Beregn vektor for laser
                double vx = XIntersection - XStart;
                double vy = YIntersection - YStart;

                // Beregn laserens vinkel i forhold til x-aksen. 
                double vLaser = Math.Atan2(vy, vx);

                // Beregn hældningen af objektets overflade, som hældning af lineær funktion ud fra to punkter på funktionen. 
                // Her bruges en lav skridtlængde på 0.001, for at finde hældningen så tæt på skæringspunktet som muligt. 
                double slopeFunction = (SurfaceFunction(XIntersection + 0.001) - SurfaceFunction(XIntersection)) / 0.001;
                double vFunction = Math.Atan(slopeFunction);

                // Beregn normalvektorens vinkel
                double vNormal = vFunction + Math.PI / 2;

                // Reflekter vektoren omkring normalvektoren
                vOut = 2 * vNormal - vLaser;

                // Normaliser vinklen til at ligge mellem -PI og PI
                vOut = NormalizeAngle(vOut);

                // Kontrollér retningen af den reflekterede vektor
                double reflectedDx = Math.Cos(vOut);
                double reflectedDy = Math.Sin(vOut);

                //Tilføjet tjek
                double dotProduct = vx * reflectedDx + vy * reflectedDy;
                if (dotProduct < 0)
                {
                    vOut = NormalizeAngle(vOut + Math.PI);
                    reflectedDx = Math.Cos(vOut);
                    reflectedDy = Math.Sin(vOut);
                }

                double normalDx = Math.Cos(vNormal);
                double normalDy = Math.Sin(vNormal);

                // Dot-produkt for at tjekke retning
                if (reflectedDx * normalDx + reflectedDy * normalDy < 0)
                {
                    // Hvis dot-produktet er negativt, vend retningen
                    vOut = NormalizeAngle(vOut + Math.PI);
                    reflectedDx = Math.Cos(vOut);
                    reflectedDy = Math.Sin(vOut);
                }

            

            // Beregn ny hældning og skæringspunkt med y-aksen
            aOut = Math.Tan(vOut);
                bOut = YIntersection - aOut * XIntersection;

                Console.WriteLine($"Spejlet funktion: y = {aOut}x + {bOut}");
                //Console.WriteLine($"Reflekteret vektor: [1, {aOut}]");

                Console.WriteLine($"Reflekteret vektor: [{reflectedDx}, {reflectedDy}]"); // Brug de allerede definerede variabler
                Console.WriteLine($"Laserens vektor: ({vx}, {vy}), vinkel: {vLaser}");
                Console.WriteLine($"Funktionens hældning: {slopeFunction}, vinkel: {vFunction}");
                Console.WriteLine($"Normalens vinkel: {vNormal}");
                Console.WriteLine($"Reflekteret vinkel: {vOut}");

                Console.WriteLine($"normalDx : {normalDx}");
                Console.WriteLine($"normalDy: {normalDy}");
                // Kontrollér retningen baseret på normalvektoren
                if (reflectedDx * normalDx + reflectedDy * normalDy < 0)
                {
                        vOut += Math.PI; // Vend retningen
                    Console.WriteLine("RETNINGEN ER BLEVET VENDT!!!!!!!");
                    Console.WriteLine($"Reflekteret vinkel er nu: {vOut}");
                    Console.WriteLine($"reflectedDx vinkel er nu: {reflectedDx}");
                    Console.WriteLine($"reflectedDy vinkel er nu: {reflectedDy}");

                }

            }
            private double NormalizeAngle(double angle)
            {
                // Normaliser vinkel til at være mellem -PI og PI
                while (angle > Math.PI) angle -= 2 * Math.PI;
                while (angle < -Math.PI) angle += 2 * Math.PI;
                return angle;
            }


        public double GetReflectedAngle()
            {
                return vOut;
            }
            public Func<double, double> GetReflectedFunction()
            {
                return x => aOut * x + bOut;
            }
        }
    }
