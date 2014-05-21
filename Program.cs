using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math.Optimization;

namespace LFD_SupportVectors
{
  class Program
  {
    static Random R = new Random();
    static void Main(string[] args)
    {
      //var wTarget = new[] { GetR(), GetR() };
      var wTarget = new[] {-0.5, 0.2, 0.9 };

      var classified = Generate(1000, wTarget);
      Console.WriteLine("{0} : {1}", classified.Count(t => t.Item2 > 0), classified.Count(t => t.Item2 <= 0));

      var wPla = DoPla(new[] { 0.0, 0.0, 0.0}, classified);

      Console.WriteLine("{0} - {1}", vToStr(wPla), vToStr(Norm(wPla)));

      Console.ReadKey(true);
    }

    private static string vToStr(double[] wPla)
    {
      return "{" + wPla.Select(x => x.ToString("f3") + " ").Aggregate((acc, s) => acc + s) + "}";
    }    

    private static double[] Norm(double[] wPla)
    {
      var mag = Math.Sqrt( wPla.Sum(x=>x*x));
      return wPla.Select(x => x / mag).ToArray();
    }

    private static double[] DoPla(double[] w, Tuple<double[], double>[] classified)
    {
      var misclassified = classified.Where(t => GetY(w, t.Item1) != t.Item2).ToArray();
      Console.WriteLine("{0} misclassified.", misclassified.Length);
      if (misclassified.Length == 0)
        return w;
      else
        return DoPla(NextW(w, misclassified[R.Next(misclassified.Length)]), classified);
    }

    private static double[] NextW(double[] w, Tuple<double[], double> wrong)
    {
      return w.Zip(wrong.Item1, (wn, xn) => wn + xn * wrong.Item2).ToArray();
    }


    private static Tuple<double[], double>[] Generate(int count, double[] w)
    {
      return Enumerable.Range(1,count)
        .Select(_ => GetX())
        .Select(x => new Tuple<double[], double>(x, GetY(x, w)))
        .ToArray();
    }

    private static double GetY(double[] x, double[] w)
    {
      return x.Zip(w, (xn, wn) => xn * wn).Sum() < 0 ? -1.0 : 1.0;
    }

    private static double[] GetX()
    {
      return new[] { 1.0, GetR(), GetR() };
    }

    private static double GetR()
    {
      return R.NextDouble() * 2 - 1;
    }
  }
}
