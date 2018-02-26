using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiAgentQLearning
{
    class Transition
    {
        private Dictionary<ProbabilityTransitionKey, List<State>> _transitionTable = new Dictionary<ProbabilityTransitionKey, List<State>>();
        private Random random = new Random();

        public Transition(StateSet states, JointActionSet actions)
        {
            foreach (State currState in states)
            {
                foreach (JointAction action in actions)
                {
                    AddNextStateProbabilities(currState, action);
                }
            }
        }

        public State GetNextState(State currState, JointAction action)
        {
            var possibleStates = _transitionTable[new ProbabilityTransitionKey(currState, action)];
            return possibleStates[random.Next(possibleStates.Count)];
        }

        private void AddNextStateProbabilities(State currState, JointAction action)
        {
            var possibleNextStates = new List<State>();

            var nextPlayerAPosition = GetNextPosition(currState.PlayerAPosition, action.currplayer1Act);
            var nextPlayerBPosition = GetNextPosition(currState.PlayerBPosition, action.player2act);

            //"When a player executes an action that would take it to the square occupied by the other player, possession of
            // the ball goes to the stationary player and the move does not take place"

            // Player A goes first and runs into player B's current position
            // or Player B goes first and runs into player A's position
            // "If the sequences of actions causes the players to collide, then only the first moves"
            if (nextPlayerAPosition == currState.PlayerBPosition && nextPlayerBPosition == currState.PlayerAPosition)
            {
                //50% chance that the player with the ball moves first in this scenario, and nobody moves.  As such, 50% chance the possession is changed to the other player.
                possibleNextStates.Add(new State(currState.PlayerAPosition, currState.PlayerBPosition, BallPossessor.A));
                possibleNextStates.Add(new State(currState.PlayerAPosition, currState.PlayerBPosition, BallPossessor.B));
            }
            //The second move will result in a collision.  Only the first move takes place and ball changes possession only if the second player possesses the ball
            else if (nextPlayerAPosition == nextPlayerBPosition)
            {
                possibleNextStates.Add(new State(currState.PlayerAPosition, nextPlayerBPosition, BallPossessor.B));
                possibleNextStates.Add(new State(nextPlayerAPosition, currState.PlayerBPosition, BallPossessor.A));
            }
            else if (nextPlayerAPosition == currState.PlayerBPosition && nextPlayerBPosition != currState.PlayerAPosition)
            {
                if (currState.Possessor == BallPossessor.A)
                {
                    possibleNextStates.Add(new State(currState.PlayerAPosition, currState.PlayerBPosition, BallPossessor.B));
                }
                else
                {
                    possibleNextStates.Add(new State(currState.PlayerAPosition, currState.PlayerBPosition, BallPossessor.A));
                }

                possibleNextStates.Add(new State(nextPlayerAPosition, nextPlayerBPosition, currState.Possessor));
            }
            else if (nextPlayerAPosition != currState.PlayerBPosition && nextPlayerBPosition == currState.PlayerAPosition)
            {
                if (currState.Possessor == BallPossessor.B)
                {
                    possibleNextStates.Add(new State(currState.PlayerAPosition, currState.PlayerBPosition, BallPossessor.A));
                }
                else
                {
                    possibleNextStates.Add(new State(currState.PlayerAPosition, currState.PlayerBPosition, BallPossessor.B));
                }

                possibleNextStates.Add(new State(nextPlayerAPosition, nextPlayerBPosition, currState.Possessor));
            }
            //No collision took place, deterministically move the players to their next locations
            else
            {
                possibleNextStates.Add(new State(nextPlayerAPosition, nextPlayerBPosition, currState.Possessor));
            }

            possibleNextStates.RemoveAll(s => s.PlayerAPosition == s.PlayerBPosition);

            _transitionTable[new ProbabilityTransitionKey(currState, action)] = possibleNextStates;
        }

        private int GetNextPosition(int currentPosition, Action action)
        {
            var nextPosition = 0;

            //Boundaries
            if (currentPosition >= 0 && currentPosition <= 3)
            {
                if (action == Action.N
                    || currentPosition == 0 && action == Action.W
                    || currentPosition == 3 && action == Action.E)
                {
                    nextPosition = currentPosition;
                }
                else
                {
                    nextPosition = currentPosition + MapActionToGridShift(action);
                }
            }
            else if (currentPosition >= 4 && currentPosition <= 7)
            {
                if (action == Action.S
                    || currentPosition == 4 && action == Action.W
                    || currentPosition == 7 && action == Action.E)
                {
                    nextPosition = currentPosition;
                }
                else
                {
                    nextPosition = currentPosition + MapActionToGridShift(action);
                }
            }
            else
            {
                nextPosition = currentPosition + MapActionToGridShift(action);
            }

            return nextPosition;
        }

        private int MapActionToGridShift(Action action)
        {
            switch (action)
            {
                case Action.N:
                    return -4;
                case Action.S:
                    return 4;
                case Action.E:
                    return 1;
                case Action.W:
                    return -1;
                case Action.X:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private class ProbabilityTransitionKey : IEquatable<ProbabilityTransitionKey>
        {
            public ProbabilityTransitionKey(State currState, JointAction action)
            {
                currState = currState;
                Action = action;
            }

            private State currState { get; }
            private JointAction Action { get; }

            public bool Equals(ProbabilityTransitionKey other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(currState, other.currState) && Equals(Action, other.Action);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ProbabilityTransitionKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = currState?.GetHashCode() ?? 0;
                    hashCode = (hashCode*397) ^ (Action?.GetHashCode() ?? 0);
                    return hashCode;
                }
            }
        }
    }
}
