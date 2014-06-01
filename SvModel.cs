using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SolverFoundation.Services;

namespace LFD_SupportVectors
{
  public class SvModel
  {
    public SvModel(ICollection<Tuple<double[], double>> classified)
    {
      if (classified.Count < 1) 
        throw new ArgumentException("Need at least 1 classified point.", "classified");
      var wLen = classified.First().Item1.Length;
      if (classified.Count(t=>t.Item1.Length != wLen) > 0)
        throw new ArgumentException("All input data vectors have to be the same length.", "classified");

      _Context = SolverContext.GetContext();
      _Model = _Context.CreateModel();
      _Model.Name = "SupportThatVector";

      //make decisions!
      Weights = classified.First().Item1
        .Select((x,i)=>new Decision(Domain.Real, string.Format("W{0}", i)))
        .ToArray();
      foreach (var w in Weights)
        _Model.AddDecision(w);

      //Add constraints!
      //    R{I} -> {YI} * (W0 
      //+ {WJ}*W{J}
      //...
      //) >= 1.0
      string.Format("    R{0} -> {3:f6} * (W0 + {1:f6}*W1 + {2:f6}*W2) >= 1.0", i, t.Item1[1], t.Item1[2], t.Item2)));


      //goal!

    }

    public Decision[] Weights;
    private SolverContext _Context;
    private Model _Model;
  }
}
