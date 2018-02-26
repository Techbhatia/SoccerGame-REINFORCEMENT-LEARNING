using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SolverFoundation.Services;

namespace MultiAgentQLearning
{
    public class CorrelatedQTable
    {
        private readonly Dictionary<keyTable, double> Q = new Dictionary<keyTable, double>();
        private readonly Dictionary<keyTable, double> Player2Q = new Dictionary<keyTable, double>();

        private readonly double gama = 0.9;
        private int totalcount;
        public double learningrate = 0.2;

        public void UpdateQValue(State currState, State nextState, Action currplayer1Act, Action player2act, double currPlayer1reward, double Player2reward, bool done)
        {
            var Qkey = new keyTable(currState, currplayer1Act, player2act);
            var currQ_val = getCurrPQvalue(currState, currplayer1Act, player2act);
            var player2CurrQ_val = getPlayer2Qval(currState, currplayer1Act, player2act);

            Tuple<double, double> newStateVal = new Tuple<double, double>(0.0, 0.0);
            if (!done)
            {
                //Update value using foe function
                newStateVal = CorrValue.GetValue(nextState, this);
            }

            var nextQValueCurrent = (1 - learningrate) * currQ_val + learningrate * (currPlayer1reward + gama * newStateVal.Item1);
            var nextQValueOpponent = (1 - learningrate) * player2CurrQ_val + learningrate * (Player2reward + gama * newStateVal.Item2);

            Q[Qkey] = nextQValueCurrent;
            Player2Q[Qkey] = nextQValueOpponent;

            //Decay learning_Rate
            ++totalcount;
            learningrate = learningrate / (1 + 0.0000000001 * ++totalcount) > 0.001 ? learningrate / (1 + 0.0000000001 * ++totalcount) : 0.001;
        }

        public double getCurrPQvalue(State state, Action currplayer1Act, Action player2act)
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

        public double getPlayer2Qval(State state, Action currplayer1Act, Action player2act)
        {
            double currQ_val;
            var Qkey = new keyTable(state, currplayer1Act, player2act);

            if (!Player2Q.TryGetValue(Qkey, out currQ_val))
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
                return Equals((keyTable) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = State?.GetHashCode() ?? 0;
                    hashCode = (hashCode*397) ^ (int) currplayer1Act;
                    hashCode = (hashCode*397) ^ (int) player2act;
                    return hashCode;
                }
            }
        }

        internal static class CorrValue
        {
            public static Tuple<double, double> GetValue(State s, CorrelatedQTable q)
            {
                var contxt = SolverContext.GetContext();
                contxt.ClearModel();
                var model = contxt.CreateModel();

                var actDecisions = new Dictionary<Tuple<Action, Action>, Decision>();

                foreach (Action currentAction in Enum.GetValues(typeof(Action)))
                {
                    foreach (Action player2Action in Enum.GetValues(typeof(Action)))
                    {
                        var decision = new Decision(Domain.RealNonnegative, currentAction.ToString() + player2Action.ToString());
                        model.AddDecisions(decision);
                        actDecisions.Add(new Tuple<Action, Action>(currentAction, player2Action), decision);
                    }
                }

                var actDecisionSum = new SumTermBuilder(25);
                foreach (var decision in actDecisions.Values)
                {
                    actDecisionSum.Add(decision);
                }

                model.AddConstraint("probSumConst", actDecisionSum.ToTerm() == 1.0);

                rationalconsts(s, q.getCurrPQvalue, actDecisions, model, "A");
                rationalityConstrPlayer2(s, q.getPlayer2Qval, actDecisions, model, "B");

                var objectSum = new SumTermBuilder(10);

                //Add my terms from my Q table to objective function
                ObjFunctermAdd(s, q, actDecisions, objectSum);

                model.AddGoal("MaximizeV", GoalKind.Maximize, objectSum.ToTerm());

                var sol = contxt.Solve(new SimplexDirective());

               

                if (sol.Quality != SolverQuality.Optimal)
                {
                    contxt.ClearModel();
                    return new Tuple<double, double>(1.0, 1.0);
                }

                double Player1nextVal = 0.0;
                double Player2nextVal = 0.0;
                foreach (Action currentAction in Enum.GetValues(typeof(Action)))
                {
                    foreach (Action player2Action in Enum.GetValues(typeof(Action)))
                    {
                        var policy = getActDecision(currentAction, player2Action, actDecisions);
                        var qValue = q.getCurrPQvalue(s, currentAction, player2Action);
                        Player1nextVal += policy.ToDouble() * qValue;
                        var player2Qv = q.getPlayer2Qval(s, currentAction, player2Action);
                        Player2nextVal += policy.ToDouble() * player2Qv;
                    }
                }

                return new Tuple<double, double>(Player1nextVal, Player2nextVal);
            }

            private static void ObjFunctermAdd(State s, CorrelatedQTable q, Dictionary<Tuple<Action, Action>, Decision> actDecisions, SumTermBuilder objectSum)
            {
                foreach (Action currentAction in Enum.GetValues(typeof(Action)))
                {
                    foreach (Action player2Action in Enum.GetValues(typeof(Action)))
                    {
                        var policy = getActDecision(currentAction, player2Action, actDecisions);
                        var qValue = q.getCurrPQvalue(s, currentAction, player2Action);

                        objectSum.Add(policy * qValue);
                    }
                }

                foreach (Action currentAction in Enum.GetValues(typeof(Action)))
                {
                    foreach (Action player2Action in Enum.GetValues(typeof(Action)))
                    {
                        var policy = getActDecision(currentAction, player2Action, actDecisions);
                        var player2Qv = q.getPlayer2Qval(s, currentAction, player2Action);

                        objectSum.Add(policy * player2Qv);
                    }
                }
            }

            private static void rationalconsts(State s, Func<State, Action, Action, double> getQ, Dictionary<Tuple<Action, Action>, Decision> actDecisions, Model model, string constPrefix)
            {
                var rotList = new Queue<Action>();
                foreach (Action action in Enum.GetValues(typeof(Action)))
                {
                    rotList.Enqueue(action);
                }

                var cons_cnt = 0;

                for (var i = 0; i < Enum.GetValues(typeof(Action)).Length; ++i)
                {
                    var constRowSum = new SumTermBuilder(5);
                    var isConstrow = true;
                    Action contextAction = Action.N;
                    
                    foreach (Action currentAction in rotList)
                    {
                        var otherRowSum = new SumTermBuilder(5);

                        if (isConstrow)
                        {
                            contextAction = currentAction;
                        }

                        foreach (Action player2Action in Enum.GetValues(typeof(Action)))
                        {
                            var policy = getActDecision(contextAction, player2Action, actDecisions);

                            if (isConstrow)
                            {
                                var qValue = getQ(s, currentAction, player2Action);

                                constRowSum.Add(policy*qValue);
                            }
                            else
                            {
                                var qValue = getQ(s, currentAction, player2Action);

                                otherRowSum.Add(policy*qValue);
                            }
                        }

                        if (!isConstrow)
                        {
                            model.AddConstraint(constPrefix + "const" + cons_cnt, constRowSum.ToTerm() >= otherRowSum.ToTerm());
                            cons_cnt++;
                        }

                        isConstrow = false;
                    }

                    //Rotate list
                    var elementToRotate = rotList.Dequeue();
                    rotList.Enqueue(elementToRotate);
                }
            }

            private static void rationalityConstrPlayer2(State s, Func<State, Action, Action, double> getQ, Dictionary<Tuple<Action, Action>, Decision> actDecisions, Model model, string constPrefix)
            {
                var rotList = new Queue<Action>();
                foreach (Action action in Enum.GetValues(typeof(Action)))
                {
                    rotList.Enqueue(action);
                }

                var cons_cnt = 0;

                for (var i = 0; i < Enum.GetValues(typeof(Action)).Length; ++i)
                {
                    var constRowSum = new SumTermBuilder(5);
                    var isConstrow = true;
                    Action contextAction = Action.N;

                    foreach (Action player2Action in rotList)
                    {
                        var otherRowSum = new SumTermBuilder(5);

                        if (isConstrow)
                        {
                            contextAction = player2Action;
                        }

                        foreach (Action currentAction in Enum.GetValues(typeof(Action)))
                        {
                            var policy = getActDecision(currentAction, contextAction, actDecisions);

                            if (isConstrow)
                            {
                                var qValue = getQ(s, currentAction, player2Action);

                                constRowSum.Add(policy * qValue);
                            }
                            else
                            {
                                var qValue = getQ(s, currentAction, player2Action);

                                otherRowSum.Add(policy * qValue);
                            }
                        }

                        if (!isConstrow)
                        {
                            model.AddConstraint(constPrefix + "const" + cons_cnt, constRowSum.ToTerm() >= otherRowSum.ToTerm());
                            cons_cnt++;
                        }

                        isConstrow = false;
                    }

                    //Rotate list
                    var elementToRotate = rotList.Dequeue();
                    rotList.Enqueue(elementToRotate);
                }
            }
        }

        private static Decision getActDecision(Action currentAction, Action player2Action, Dictionary<Tuple<Action, Action>, Decision> actDecisions)
        {
            var key = new Tuple<Action, Action>(currentAction, player2Action);
            return actDecisions[key];
        }
    }
}
