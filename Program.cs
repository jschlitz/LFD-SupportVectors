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
    const int TRIALS = 10;
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

      for (int i = 0; i < 1000; i++)
      {
        var testData = Generate(TRIALS * 10, wTarget).ToArray();
        var pla = GetWithPla(wTarget, classified);
        plas.Add(Wrongness(wTarget, pla, testData));

        //var sv = DoSv(classified);
        //svs.Add(Wrongness(wTarget, sv, testData));

        if (svs.Last() > pla.Last()) 
          better += 1.0;
      }

      Console.WriteLine("pla={0:f2}, sv={1:f2}, betterness={2}", plas.Average() * 100, svs.Average() * 100, better*100/1000);
      
      Console.ReadKey(true);
    }

    private static double[] DoSv(Tuple<double[], double>[] classified)
    {
      return null;
      //nOPE.
      //var w = new double[classified.First().Item1.Length];
      ////fuck it.
      //double w0=0;
      //double w1=0;
      //double w2=0;
      //var objective = new QuadraticObjectiveFunction(() => 0.5 * (w0 * w0) + 0.5 * (w1 * w1) + 0.5 * (w2 * w2));

      //var lcc = new LinearConstraintCollection(
      //  classified
      //    .Select(item => new LinearConstraint(objective, () => item.Item2 * (item.Item1[0]*w0 + item.Item1[1]*w1 + item.Item1[2]*w2) >= 1)));

      //var solver = new GoldfarbIdnaniQuadraticSolver(w.Length, lcc);
      //solver.Minimize(objective);

      //return Norm(solver.Solution);

      
      //fuck this alpha bullshit. see if I can just feed 0.5 * dot(w,w) s.t. yn(dot(w,t) >= 1. and fuck b. leave it in w. see what fucking happens. fuck.
      //var stripped = classified.Select(t => new Tuple<double[], double>(t.Item1.Skip(1).ToArray(), t.Item2))
      //  .ToArray();
      //var q = GetQ(stripped);

      ////all are 0 or more
      //var lcc = new LinearConstraintCollection(
      //  Enumerable.Range(0, stripped.Length)
      //    .Select(i => new LinearConstraint(1)
      //    {
      //      VariablesAtIndices = new[] { i },
      //      ShouldBe = ConstraintType.GreaterThanOrEqualTo,
      //      Value = 0.0,
      //      CombinedAs= new []{1.0} //???
      //    }));
      ////and they zero out with the classificaiton
      //lcc.Add(
      //  //new LinearConstraint(stripped.Select(t => t.Item2).ToArray()) { Value = 0, ShouldBe = ConstraintType.EqualTo }
      //  new LinearConstraint(stripped.Length)
      //  {
      //    Value = 0,
      //    ShouldBe = ConstraintType.EqualTo,
      //    VariablesAtIndices = Enumerable.Range(0, stripped.Length).ToArray(),
      //    CombinedAs = stripped.Select(t => t.Item2).ToArray()
      //  }        );

      //var solver = new GoldfarbIdnaniQuadraticSolver(stripped.Length, lcc);
      //solver.Minimize(q, neg1Array(stripped.Length));
      ////b  = 1/y - w_*x_ 

      //var tmp = solver.Solution.Zip(stripped, (alpha, s) => s.Item1.Select(x => x * alpha * s.Item2))
      //  .Aggregate(VAdd).ToList();
      //var support = solver.Solution.Zip(stripped, (alpha, s) => Math.Abs(alpha) > 0.0001 ? s : null).Where(x => x != null).ToArray();
      //var offset = GetOffset(support, tmp); //I'm pretty sure this calculation for b is wrong: you get a different one for each of elements...
      //tmp.Insert(0, offset);
      //return Norm( tmp.ToArray());


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
