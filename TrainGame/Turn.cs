﻿using TrainGame.Players;
using TrainGame.Rules;

namespace TrainGame
{
    public class Turn
    {
        public Turn(Player player, PlayerAction move)
        {
            By = player;
            Did = move;
        }

        public Player By { get; }
        public PlayerAction Did { get; }
    }
}