using System;
using System.Collections.Generic;

namespace MultiAgentQLearning
{
    public class QLearnerQTable
    {
        private readonly Dictionary<keyTable, double> Q = new Dictionary<keyTable, double>();

        private readonly double gama = 0.9;
        private int totalcount;
        private double learningrate_init = 0.001;

        //private double learning_Rate => learningrate_init/(1 + 0.00001 * ++totalcount) > 0.001 ? learningrate_init / (1 + 0.00001 * ++totalcount) : 0.001;
        private double learning_Rate => 1/(1 + ++totalcount) > 0.001 ? 1 / (1 +  ++totalcount) : 0.001;

        public double UpdateQValue(State state, State nextState, Action currplayer1Act, double currPlayer1reward)
        {
            double currQ_val;
            var Qkey = new keyTable(state, currplayer1Act);

            if (!Q.TryGetValue(Qkey, out currQ_val))
            {
                //Default Q Value is 1.0
                currQ_val = 1.0;
            }

            //Update value table with current state
            var nextStateV = getMaxQval(nextState);

            //Q value update
            var updatedQValue = (1 - learning_Rate) * currQ_val + learning_Rate * (currPlayer1reward + gama * nextStateV);

            Q[Qkey] = updatedQValue;

            return updatedQValue;
        }

        public double getQval(State state, Action playerAction)
        {
            double qValue;
            if (!Q.TryGetValue(new keyTable(state, playerAction), out qValue))
            {
                //Defaults to 1.0
                qValue = 1.0;
            }

            return qValue;
        }

        private double getMaxQval(State nextState)
        {
            var maxQval = double.MinValue;

            foreach (Action action in Enum.GetValues(typeof(Action)))
            {
                double currQ_val;
                var Qkey = new keyTable(nextState, action);

                if (!Q.TryGetValue(Qkey, out currQ_val))
                {
                    //Default Q Value is 1.0
                    currQ_val = 1.0;
                }

                maxQval = maxQval > currQ_val ? maxQval : currQ_val;
            }

            return maxQval;
        }

        class keyTable : IEquatable<keyTable>
        {
            private State State { get; }
            private Action currplayer1Act { get; }

            public keyTable(State state, Action currplayer1Act)
            {
                State = state;
                currplayer1Act = currplayer1Act;
            }

            public bool Equals(keyTable other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(State, other.State) && currplayer1Act == other.currplayer1Act;
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
                    return hashCode;
                }
            }
        }
    }
}
