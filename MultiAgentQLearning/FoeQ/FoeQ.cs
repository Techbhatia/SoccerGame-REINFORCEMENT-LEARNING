using System;
using System.Collections.Generic;
using Microsoft.SolverFoundation.Services;

namespace MultiAgentQLearning
{
    public class FoeQTable
    {
        private readonly Dictionary<keyTable, double> Q = new Dictionary<keyTable, double>();

        private readonly double gama = 0.9;
        private int totalcount;
        private double learningrate_init = 0.2;

        private double learning_Rate => learningrate_init / (1 + 0.00001 * ++totalcount) > 0.001 ? learningrate_init / (1 + 0.00001 * ++totalcount) : 0.001;

        public void UpdateQValue(State currState, State nextState, Action currplayer1Act, Action player2act, double currPlayer1reward)
        {
            double currQ_val;
            var Qkey = new keyTable(currState, currplayer1Act, player2act);

            if (!Q.TryGetValue(Qkey, out currQ_val))
            {
                //Default Q Value is 1.0
                currQ_val = 1.0;
            }

            //Update value using foe function
            var newStateVal = FoeQVal.GetValue(nextState, this);

            var newQVal = (1 - learning_Rate) * currQ_val + learning_Rate * (currPlayer1reward + gama * newStateVal);

            Q[Qkey] = newQVal;
        }

        public double getQval(State state, Action currplayer1Act, Action player2act)
        {
            double currQ_val;
            var Qkey = new keyTable(state, currplayer1Act, player2act);

            if (!Q.TryGetValue(Qkey, out currQ_val))
            {
                //Default Q Value is 1.0
                currQ_val = 1.0;
            }

            return currQ_val;
        }
      
        class keyTable : IEquatable<keyTable>
        {
            private State State { get; }
            private Action currplayer1Act { get; }
            private Action player2act { get; }

            public keyTable(State state, Action currplayer1Act, Action player2act)
            {
                State = state;
                currplayer1Act = currplayer1Act;
                player2act = player2act;
            }

            public bool Equals(keyTable other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(State, other.State) && currplayer1Act == other.currplayer1Act && player2act == other.player2act;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((keyTable)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (State != null ? State.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int)currplayer1Act;
                    hashCode = (hashCode * 397) ^ (int)player2act;
                    return hashCode;
                }
            }
        }

        class FoeQVal
        {
            public static double GetValue(State s, FoeQTable q)
            {
                var contxt = SolverContext.GetContext();
                var model = contxt.CreateModel();

                var actDecisions = new List<Decision>();

                foreach (var action in Enum.GetNames(typeof(Action)))
                {
                    var decision = new Decision(Domain.RealNonnegative, action);
                    model.AddDecision(decision);
                    actDecisions.Add(decision);
                }

                var val_Decis = new Decision(Domain.Real, "value");
                model.AddDecision(val_Decis);

                model.AddConstraint("probSumConst", actDecisions[0] + actDecisions[1] + actDecisions[2] + actDecisions[3] + actDecisions[4] == 1.0);

                int cons_cnt = 0;

                foreach (Action Player2 in Enum.GetValues(typeof(Action)))
                {
                    var qConstVals = new List<double>();

                    foreach (Action currentPlayer in Enum.GetValues(typeof(Action)))
                    {
                        qConstVals.Add(q.getQval(s, currentPlayer, Player2));
                    }

                    model.AddConstraint("Const" + cons_cnt, qConstVals[0] * actDecisions[0] + qConstVals[1] * actDecisions[1] + qConstVals[2] * actDecisions[2] + qConstVals[3] * actDecisions[3] + qConstVals[4] * actDecisions[4] >= val_Decis);

                    ++cons_cnt;
                }

                model.AddGoal("MaximizeV", GoalKind.Maximize, val_Decis);

                contxt.Solve(new SimplexDirective());

                contxt.ClearModel();

                return val_Decis.GetDouble();
            }
        }
    }
}
