using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Entities.Utilities.Cloning;

namespace FantasyFootballRobot.Core.Entities
{
    [Serializable]
    public class Player : IDeepCloneable<Player>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClubCode { get; set; }
        public Position Position { get; set; }
        public Status Status { get; set; }

        public IList<FutureFixture> FutureFixtures { get; set; }
        public IList<PastFixture> PastFixtures { get; set; }
        public IList<Season> PastSeasons { get; set; }

        public int OriginalCost { get; set; }
        public int NowCost { get; set; }

        public Season GetPastSeason(int endYear)
        {
            return PastSeasons.SingleOrDefault(ps => ps.SeasonEndYear == endYear);
        }

        public Player DeepClone()
        {          
            return ExpressionTreeCloner.DeepFieldClone(this);
        }
    }
}