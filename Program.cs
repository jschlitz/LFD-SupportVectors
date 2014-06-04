using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LFD_SupportVectors
{
  class Program
  {
    static Random R = new Random();
    const int TRIALS = 100;
    static void Main(string[] args)
    {

      //var twoD = new int[2, 3];
      //twoD[0, 0] = 1;
      //twoD[0, 1] = 2;
      //twoD[0, 2] = 3;
      //twoD[1, 0] = 4;
      //twoD[1, 1] = 5;
      //twoD[1, 2] = 6;
      //var arrArr = Enumerable.Range(0, 2).Select(r => Enumerable.Range(0, 3).Select(c => 1 + r * 3 + c).ToArray()).ToArray();
      ////arrArr != twoD

      //var wTarget = new[] { GetR(), GetR() };
      var wTarget = Norm( new[] {0.1, 0.2, 0.9 });

      var classified = Generate(TRIALS, wTarget);
      //Console.WriteLine("{0} : {1}", classified.Count(t => t.Item2 > 0), classified.Count(t => t.Item2 <= 0));

      var plas = new List<double>();
      var svs = new List<double>();
      double better = 0;

      var s = string.Join(",\r", classified.Select(
        (t, i) =>
//          string.Format("    X1{0}={1:f6}, X2{0}={2:f6}, Y{0}={3:f1}", i, t.Item1[1], t.Item1[2], t.Item2)));
          string.Format("    R{0} -> {3:f6} * (W0 + {1:f6}*W1 + {2:f6}*W2) >= 1.0", i, t.Item1[1], t.Item1[2], t.Item2)));

      var svCount = 0;
      for (int i = 0; i < 1000; i++)
      {
        var testData = Generate(TRIALS * 10, wTarget).ToArray();
        var pla = GetWithPla(wTarget, classified);
        plas.Add(Wrongness(wTarget, pla, testData));

        var sv = DoSv(classified);
        svs.Add(Wrongness(wTarget, sv, testData));
        var xw  = classified.Select(t => t.Item2 * Dot(t.Item1, sv)).ToArray();
        var tmp = xw.Min(x=>Math.Abs(x));
        svCount += xw.Count(x => (x / tmp) - 1.0 <0.001);


        if (svs.Last() < pla.Last()) 
          better += 1.0;
      }

      Console.WriteLine("pla={0:f2}, sv={1:f2}, betterness={2}, svCount={3}", plas.Average() * 100, svs.Average() * 100, better, ((double)svCount) /1000.0);
      
      Console.ReadKey(true);
    }

    private static double[] DoSv(Tuple<double[], double>[] classified)
    {
      var sv = new SvModel(classified);
      sv.Solve();
      var wVec = Norm(sv.Weights.Select(d => d.ToDouble()).ToArray());

      return wVec;

    }

    private static double GetOffset(Tuple<double[], double>[] stripped, List<double> tmp)
    {
      //these should all be the same, but they ain't dammit. Am I misunderstanding things, or are doubles this crappy in this situation?
      var foo = stripped.Select(s => 1 / s.Item2 - Dot(tmp.ToArray(), s.Item1)).ToArray();
      return foo.Average(); //.First();
    }
    private static double[] VAdd(IEnumerable<double> x, IEnumerable<double> y)
    {
      return x.Zip(y, (xn, yn) => xn + yn).ToArray();
    }

    private static double[] neg1Array(int length)
    {
      return Enumerable.Range(1, length).Select(_ => -1.0).ToArray();
    }
    

    public static double[,] GetQ(Tuple<double[], double>[] stripped)
    {
      var result = new double[stripped.Length, stripped.Length];
      for (int r = 0; r < stripped.Length; r++)
      {
        for (int c = 0; c < stripped.Length; c++)
        {
          //TODO: do I have to multiply x2 on the diagonal?
          result[r, c] = stripped[r].Item2 * stripped[c].Item2 * Dot(stripped[r].Item1, stripped[c].Item1) *(r == c ? 2 : 1);
        }
      }
      
      return result;
    }

    private static double[] GetWithPla(double[] wTarget, Tuple<double[], double>[] classified)
    {
      var wPla = DoPla(new[] { 0.0, 0.0, 0.0 }, classified);
      //Console.WriteLine();

      //Console.WriteLine("{0} - {1}", vToStr(wPla), vToStr(Norm(wPla)));
      wPla = Norm(wPla);

      return wPla;
      //test it.
      //Console.WriteLine("{0}% wrong.", 100.0 * wrong / testData.Length);
    }

    private static double Wrongness(double[] wTarget, double[] guess, Tuple<double[], double>[] testData)
    {
      var wrong = testData.Where(t => GetY(guess, t.Item1) != t.Item2).ToArray().Count();
      return ((double)wrong) / testData.Length;
    }

    private static string vToStr(double[] wPla)
    {
      return "{" + wPla.Select(x => x.ToString("f3") + " ").Aggregate( (acc, s) => acc + s) + "}";
    }    

    private static double[] Norm(double[] wPla)
    {
      var mag = Math.Sqrt( wPla.Sum(x=>x*x));
      return wPla.Select(x => x / mag).ToArray();
    }

    private static double[] DoPla(double[] w, Tuple<double[], double>[] classified)
    {
      var misclassified = classified.Where(t => GetY(w, t.Item1) != t.Item2).ToArray();
      //Console.WriteLine("{0} misclassified.", misclassified.Length);
      //Console.Write(".");
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
      return Dot(x, w) < 0 ? -1.0 : 1.0;
    }

    private static double Dot(double[] x, double[] w)
    {
      return x.Zip(w, (xn, wn) => xn * wn).Sum();
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
