namespace MultiAgentQLearning
{
    class Rewards
    {
        public int GetPlayerAReward(State s)
        {
            var reward = 0;

            //I score
            if ((s.PlayerAPosition == 0 || s.PlayerAPosition == 4)
                && s.Possessor == BallPossessor.A)
            {
                reward = 100;
            }
            //Other player scores
            else if ((s.PlayerBPosition ==  3 || s.PlayerBPosition == 7)
                        && s.Possessor == BallPossessor.B)
            {
                reward = -100;
            }
            //Other players in my goal with ball
            else if ((s.PlayerBPosition == 0 || s.PlayerBPosition == 4)
                         && s.Possessor == BallPossessor.B)
            {
                reward = 100;
            }
            //I'm in other players goal with ball
            else if ((s.PlayerAPosition == 3 || s.PlayerAPosition == 7)
                        && s.Possessor == BallPossessor.A)
            {
                reward = -100;
            }

            return reward;
        }

        public int GetPlayerBReward(State s)
        {
            var reward = 0;

            //I score
            if ((s.PlayerBPosition == 3 || s.PlayerBPosition == 7)
                        && s.Possessor == BallPossessor.B)
            {
                reward = 100;
            }
            //Other player scores
            else if ((s.PlayerAPosition == 0 || s.PlayerAPosition == 4)
                && s.Possessor == BallPossessor.A)
            {
                reward = -100;
            }
            //Other players in my goal with ball
            else if ((s.PlayerAPosition == 3 || s.PlayerAPosition == 7)
                         && s.Possessor == BallPossessor.A)
            {
                reward = 100;
            }
            //I'm in other players goal with ball
            else if ((s.PlayerBPosition == 0 || s.PlayerBPosition == 4)
                        && s.Possessor == BallPossessor.B)
            {
                reward = -100;
            }

            return reward;
        }
    }
}
