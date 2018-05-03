using System;
using System.Collections.Generic;
using System.Linq;

namespace TrainGame.Players
{
    public class HumanPlayer : Player
    {
        protected IRender Ui;

        public HumanPlayer()
            :base()
        {
        }

        public override string DecideName() => Ui.Ask("Enter your name");

        public override PlayerAction DecideAction(Game current)
        {
            Ui.Render(current, this);
            return Ui.Pick(Enum.GetValues(typeof(PlayerAction)).Cast<PlayerAction>());
        }

        public override IEnumerable<DestinationCard> DecideDestinations(IEnumerable<DestinationCard> choices, Game current)
        {
            Ui.Render(current, this);
            return Ui.Pick(choices, current.RuleSet.DestinationDrawMinimum, choices.Count());
        }
        
        public override Route NextClaim(Game current)
        {
            Ui.Render(current, this);
            return Ui.Pick(current.Board.AvailableRoutes());
        }

        public override TrainCard DecideTicket(TrainCard[] from, Game current)
        {
            Ui.Render(current, this);
            return Ui.Pick(from, 0, 1).SingleOrDefault();
        }
    }
}
