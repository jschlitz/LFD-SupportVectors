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
    public SvModel(IList<Tuple<double[], double>> classified)
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
      {
        _Model.AddDecision(w);
      }

      for (int i =0; i< classified.Count; i++)
	    {
        var item = classified[i];
        var sb = new StringBuilder();
        sb.AppendFormat("R{0} -> {1} * (W0 ", i, item.Item2, item.Item1[0]);
        for (int j = 1; j < item.Item1.Length; j++)
        {
          sb.AppendFormat("+ ({0}*W{j})", item.Item1[j]);
        }
        sb.Append(") >= 1.0");
        _Model.AddConstraint("R"+i, sb.ToString());
      }

      //goal!
      string[] goal = Weights.Select((_,i) => string.Format("W{0}*W{0}", i))
        .ToArray();
      _Model.AddGoal("OBJxFUNC", GoalKind.Minimize, string.Join(" + ", goal));
    }

    public virtual void Solve()
    {
      _Context.Solve();
    }

    public Decision[] Weights;
    private SolverContext _Context;
    private Model _Model;
  }
}
