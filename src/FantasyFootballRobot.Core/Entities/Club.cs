using System;
using System.Collections.Generic;
using System.Linq;
using FantasyFootballRobot.Core.Exceptions;

namespace FantasyFootballRobot.Core.Entities
{
    public class Club
    {
        private Club(string name, string code)
        {
            Code = code;
            Name = name;
        }

        public string Name { get; private set; }
        public string Code { get; private set; }

        public static IList<Club> AllClubs
        {
            get
            {
                return new List<Club>
                           {
                               Arsenal,
                               AstonVilla,
                               Blackburn,
                               Bolton,
                               Chelsea,
                               Everton,
                               Fulham,
                               Liverpool,
                               ManCity,
                               ManUtd,
                               Newcastle,
                               Norwich,
                               QPR,
                               StokeCity,
                               Sunderland,
                               Swansea,
                               Tottenham,
                               WestBrom,
                               Wigan,
                               Wolves,

                               Reading,
                               WestHam,
                               Southampton
                           };
            }
        }

        public static readonly Club Arsenal = new Club("Arsenal", "ARS");
        public static readonly Club AstonVilla = new Club("Aston Villa", "AVL");
        public static readonly Club Blackburn = new Club("Blackburn", "BLA");
        public static readonly Club Bolton = new Club("Bolton", "BOL");
        public static readonly Club Chelsea = new Club("Chelsea", "CHE");
        public static readonly Club Everton = new Club("Everton", "EVE");
        public static readonly Club Fulham = new Club("Fulham", "FUL");
        public static readonly Club Liverpool = new Club("Liverpool", "LIV");
        public static readonly Club ManCity = new Club("Man City", "MCI");
        public static readonly Club ManUtd = new Club("Man Utd", "MUN");
        public static readonly Club Newcastle = new Club("Newcastle", "NEW");
        public static readonly Club Norwich = new Club("Norwich", "NOR");
        public static readonly Club QPR = new Club("QPR", "QPR");
        public static readonly Club StokeCity = new Club("Stoke City", "STO");
        public static readonly Club Sunderland = new Club("Sunderland", "SUN");
        public static readonly Club Swansea = new Club("Swansea", "SWA");
        public static readonly Club Tottenham = new Club("Tottenham", "TOT");
        public static readonly Club WestBrom = new Club("West Brom", "WBA");
        public static readonly Club Wigan = new Club("Wigan", "WIG");
        public static readonly Club Wolves = new Club("Wolves", "WOL");

        public static readonly Club Reading = new Club("Reading", "RDG");
        public static readonly Club WestHam = new Club("West Ham", "WHU");
        public static readonly Club Southampton = new Club("Southampton", "SOU");


        public static string GetCodeFromTeamName(string name)
        {
            var team = AllClubs.SingleOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (team == null)
            {
                throw new TeamNotFoundException("Club with name " + name + " not found");
            }
            return team.Code;
        }

    }
}
