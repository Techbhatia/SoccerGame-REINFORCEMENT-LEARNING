using System;
using System.Collections;
using System.Collections.Generic;

namespace MultiAgentQLearning
{
    public class JointActionSet : IEnumerable
    {
        private readonly List<JointAction> jointActSet = new List<JointAction>();
        private readonly Random random = new Random();
        private readonly double gama = 0.9;
        private int totalcount;
        private double epsilon_Initialization = 0.5;

        private double rar => 1;

        public JointActionSet()
        {
            foreach (Action playerAAction in Enum.GetValues(typeof(Action)))
            {
                foreach (Action playerBAction in Enum.GetValues(typeof(Action)))
                {
                    jointActSet.Add(new JointAction(playerAAction, playerBAction));
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return jointActSet.GetEnumerator();
        }

        public Action GetNextAction()
        {
                Array values = Enum.GetValues(typeof(Action));
                return (Action) values.GetValue(random.Next(values.Length));
        }

        public JointAction GetNextJointAction()
        {
            Array values = Enum.GetValues(typeof(Action));
            return new JointAction((Action)values.GetValue(random.Next(values.Length)), (Action)values.GetValue(random.Next(values.Length)));
        }
    }

    public class JointAction : IEquatable<JointAction>
    {
        public Action currplayer1Act { get; }
        public Action player2act { get; }

        public JointAction(Action currplayer1Act, Action player2act)
        {
            currplayer1Act = currplayer1Act;
            player2act = player2act;
        }

        public bool Equals(JointAction other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return currplayer1Act == other.currplayer1Act && player2act == other.player2act;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JointAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) currplayer1Act*397) ^ (int) player2act;
            }
        }
    }

    public enum Action
    {
        N,
        S,
        E,
        W,
        X
    }
}
