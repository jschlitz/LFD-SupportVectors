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
      var wTarget = new[] {0.0, -0.09, 0.99 };

      var classified = Generate(1000, wTarget);
      Console.WriteLine("{0} : {1}", classified.Count(t => t.Item2 > 0), classified.Count(t => t.Item2 <= 0));
      Console.ReadKey(true);
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
