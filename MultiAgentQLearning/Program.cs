using System;
using System.Collections.Generic;
using System.IO;

namespace MultiAgentQLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter a Q learning algorithm to run for \"Soccer\": ");
                Console.WriteLine("1) Plain Q Learning");
                Console.WriteLine("2) Friend-Q");
                Console.WriteLine("3) Foe-Q");
                Console.WriteLine("4) Correlated-Q");

                var entry = Console.ReadLine();

                Console.WriteLine("Results will be written to output.csv");

                if (entry == "1")
                {
                    RunSimpleQLearning();
                }
                else if (entry == "2")
                {
                    RunFriendQ();
                }
                else if (entry == "3")
                {
                    RunFoeQ();
                }
                else if (entry == "4")
                {
                    RunCorrelatedQ();
                }
            }
        }

        private static void RunFoeQ()
        {
            var S = new StateSet();
            var A = new JointActionSet();
            var P = new Transition(S, A);
            var Q_A = new FoeQTable();
            var R = new Rewards();

            var ERR = new Dictionary<int, double>();

            var j = 0;

            while (j <= 1000000)
            {     
                //Initialize state according to Figure 4
                var initialState = new State(2, 1, BallPossessor.B);
                var done = false;
                var currState = new State(3, 1, BallPossessor.B);

                var actions = A.GetNextJointAction();

                var playerAAction = actions.currplayer1Act;
                var playerBAction = actions.player2act;

                while (!done)
                {
                    if (j % 20000 == 0) Console.WriteLine(j);

                    var q_fig_4_initial = Q_A.getQval(initialState, Action.S, Action.X);

                    var nextState = P.GetNextState(currState, new JointAction(playerAAction, playerBAction));

                    var playerAReward = R.GetPlayerAReward(nextState);
                    var playerBReward = R.GetPlayerBReward(nextState);

                    if (playerAReward != 0.0 || playerBReward != 0.0)
                        done = true;

                    Q_A.UpdateQValue(currState, nextState, playerAAction, playerBAction, playerAReward);

                    if (currState.Equals(initialState) && playerAAction == Action.S && playerBAction == Action.X)
                    {
                        var q_fig_4 = Q_A.getQval(initialState, Action.S, Action.X);
                        var diff = Math.Abs(q_fig_4 - q_fig_4_initial);

                        ERR.Add(j, diff);
                    }

                    currState = nextState;

                    actions = A.GetNextJointAction();

                    playerAAction = actions.currplayer1Act;
                    playerBAction = actions.player2act;

                    ++j;
                }
            }

            using (StreamWriter sw = File.CreateText("output.csv"))
            {
                foreach (var kvp in ERR)
                {
                    sw.WriteLine(kvp.Key + "," + kvp.Value);
                }
            }
        }

        private static void RunFriendQ()
        {
            var S = new StateSet();
            var A = new JointActionSet();
            var P = new Transition(S, A);
            var Q_A = new FriendQTable();
            var R = new Rewards();

            var ERR = new Dictionary<int, double>();

            var j = 0;

            while (j <= 1000000)
            {
                //Initialize state according to Figure 4
                var initialState = new State(2, 1, BallPossessor.B);
                var done = false;
                var currState = new State(3, 1, BallPossessor.B);

                var actions = A.GetNextJointAction();

                var playerAAction = actions.currplayer1Act;
                var playerBAction = actions.player2act;

                while (!done)
                {
                    if (j % 20000 == 0) Console.WriteLine(j);

                    var q_fig_4_initial = Q_A.getQval(initialState, Action.S, Action.X);

                    var nextState = P.GetNextState(currState, new JointAction(playerAAction, playerBAction));

                    var playerAReward = R.GetPlayerAReward(nextState);
                    var playerBReward = R.GetPlayerBReward(nextState);

                    Q_A.UpdateQval(currState, nextState, playerAAction, playerBAction, playerAReward);

                    if (playerAReward != 0.0 || playerBReward != 0.0)
                        done = true;

                    if (currState.Equals(initialState) && playerAAction == Action.S && playerBAction == Action.X)
                    {
                        var q_fig_4 = Q_A.getQval(initialState, Action.S, Action.X);
                        var diff = Math.Abs(q_fig_4 - q_fig_4_initial);

                        ERR.Add(j, diff);
                    }

                    currState = nextState;

                    actions = A.GetNextJointAction();

                    playerAAction = actions.currplayer1Act;
                    playerBAction = actions.player2act;

                    ++j;
                }
            }

            using (StreamWriter sw = File.CreateText("output.csv"))
            {
                foreach (var kvp in ERR)
                {
                    sw.WriteLine(kvp.Key + "," + kvp.Value);
                }
            }
        }

        private static void RunSimpleQLearning()
        {
            var S = new StateSet();
            var A = new JointActionSet();
            var P = new Transition(S, A);
            var Q_A = new QLearnerQTable();
            var R = new Rewards();

            var ERR = new Dictionary<int, double>();

            var j = 0;

            while (j <= 1000000)
            {
                //Initialize state according to Figure 4
                var initialState = new State(2, 1, BallPossessor.B);
                var done = false;
                var currState = initialState;

                var playerAAction = A.GetNextAction();
                var playerBAction = A.GetNextAction();

                while (!done)
                {
                    if (j % 20000 == 0) Console.WriteLine(j);

                    var q_fig_4_initial = Q_A.getQval(initialState, Action.S);

                    var nextState = P.GetNextState(currState, new JointAction(playerAAction, playerBAction));

                    var playerAReward = R.GetPlayerAReward(nextState);
                    var playerBReward = R.GetPlayerBReward(nextState);

                    Q_A.UpdateQValue(currState, nextState, playerAAction, playerAReward);

                    if (playerAReward != 0.0 || playerBReward != 0.0)
                        done = true;

                    if (currState.Equals(initialState) && playerAAction == Action.S)
                    {
                        var q_fig_4 = Q_A.getQval(initialState, Action.S);
                        var diff = Math.Abs(q_fig_4 - q_fig_4_initial);

                        ERR.Add(j, diff);
                    }

                    currState = nextState;

                    playerAAction = A.GetNextAction();
                    playerBAction = A.GetNextAction();

                    ++j;
                }
            }

            using (StreamWriter sw = File.CreateText("output.csv"))
            {
                foreach (var kvp in ERR)
                {
                    sw.WriteLine(kvp.Key + "," + kvp.Value);
                }
            }
        }

        private static void RunCorrelatedQ()
        {
            var S = new StateSet();
            var A = new JointActionSet();
            var P = new Transition(S, A);
            var Q_Joint = new CorrelatedQTable();
            var R = new Rewards();

            var ERR = new Dictionary<int, double>();

            var j = 0;

            while (j <= 1000000)
            {
                //Initialize state according to Figure 4
                var initialState = new State(2, 1, BallPossessor.B);
                var done = false;
                var currState = initialState;

                var actions = A.GetNextJointAction();

                var playerAAction = actions.currplayer1Act;
                var playerBAction = actions.player2act;

                while (!done)
                {
                    if (j % 20000 == 0) Console.WriteLine(j);

                    var q_fig_4_initial = Q_Joint.getCurrPQvalue(initialState, Action.S, Action.X);

                    var nextState = P.GetNextState(currState, new JointAction(playerAAction, playerBAction));

                    var playerAReward = R.GetPlayerAReward(nextState);
                    var playerBReward = R.GetPlayerBReward(nextState);

                    if (playerAReward != 0.0 || playerBReward != 0.0)
                        done = true;

                    Q_Joint.UpdateQValue(currState, nextState, playerAAction, playerBAction, playerAReward, playerBReward, done);

                    if (currState.Equals(initialState) && playerAAction == Action.S && playerBAction == Action.X)
                    {
                        var q_fig_4 = Q_Joint.getCurrPQvalue(initialState, Action.S, Action.X);
                        var diff = Math.Abs(q_fig_4 - q_fig_4_initial);
                        Console.WriteLine(diff);
                        ERR.Add(j, diff);
                    }

                    currState = nextState;

                    actions = A.GetNextJointAction();

                    playerAAction = actions.currplayer1Act;
                    playerBAction = actions.player2act;

                    ++j;
                }
            }

            using (StreamWriter sw = File.CreateText("output.csv"))
            {
                foreach (var kvp in ERR)
                {
                    sw.WriteLine(kvp.Key + "," + kvp.Value);
                }
            }
        }
    }
}
