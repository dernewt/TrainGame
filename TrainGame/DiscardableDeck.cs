﻿using System;
using System.Collections.Generic;
using TrainGame.Rules;
using System.Linq;

namespace TrainGame
{
    public class DiscardableDeck<T> : Deck<T>
    {
        public List<T> Discard { get; }

        public DiscardableDeck(Random entropy, Rule ruleSet, int capacity) : base(entropy, ruleSet, capacity)
        {
            Discard = new List<T>(capacity);
        }

        public DiscardableDeck(Random entropy, Rule ruleSet, IEnumerable<T> data) : base(entropy, ruleSet, data)
        {
            Discard = new List<T>(Capacity);
        }

        public override T Draw()
        {
            return Draw(1).Single();
        }

        public override IEnumerable<T> Draw(int count)
        {
            if(count > Count)
                Shuffle();

            return base.Draw(count);
        }

        public override bool CanDraw(int count = 1, Func<T, bool> having = null)
            => having == null ? Discard.Count+Count >= 1 : this.Concat(Discard).Count(having) >= 1;

        public override void Shuffle()
        {
            AddRange(Discard);
            Discard.Clear();
            base.Shuffle();
        }
    }
}
