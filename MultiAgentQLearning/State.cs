using System;
using System.Collections;
using System.Collections.Generic;

namespace MultiAgentQLearning
{
    public class StateSet : IEnumerable
    {
        public const int GridSize = 8;
        private readonly IList<State> _states = new List<State>();

        public StateSet()
        {
            for (var playerAPos = 0; playerAPos < GridSize; ++playerAPos)
            {
                for (var playerBPos = 0; playerBPos < GridSize; ++playerBPos)
                {
                    foreach (BallPossessor possession in Enum.GetValues(typeof(BallPossessor)))
                    {
                        if (playerAPos != playerBPos)
                        {
                            _states.Add(new State(playerAPos, playerBPos, possession));
                        }
                    }
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _states.GetEnumerator();
        }
    }

    public class State : IEquatable<State>
    {
        public State(int playerAPosition, int playerBPosition, BallPossessor possessor)
        {
            PlayerAPosition = playerAPosition;
            PlayerBPosition = playerBPosition;
            Possessor = possessor;
        }

        public bool Equals(State other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Possessor == other.Possessor && PlayerAPosition == other.PlayerAPosition && PlayerBPosition == other.PlayerBPosition;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((State) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Possessor;
                hashCode = (hashCode*397) ^ PlayerAPosition;
                hashCode = (hashCode*397) ^ PlayerBPosition;
                return hashCode;
            }
        }

        public BallPossessor Possessor { get; }
        public int PlayerAPosition { get; }
        public int PlayerBPosition { get; }
    }

    public enum BallPossessor
    {
        A,
        B
    }
}
