using TrainGame.Players;
using TrainGame.Rules;

namespace TrainGame
{
    public class Turn
    {
        public Turn(Player player, PlayerAction move, string moveInfo)
        {
            By = player;
            Did = move;
            MoveInfo = moveInfo;
        }

        public Player By { get; }
        public PlayerAction Did { get; }
        protected string MoveInfo { get; }

    public string DidDescription => $"{Did} {MoveInfo}";
    }
}