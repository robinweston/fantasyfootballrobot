using AutoMapper;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Entities.Json;
using Microsoft.Practices.Unity;

namespace FantasyFootballRobot.Core.Mapping
{
    public static class MappingConfiguration
    {
        public static void Bootstrap(IUnityContainer unityContainer)
        {
            Mapper.Initialize(
                x =>
                {
                    x.ConstructServicesUsing(type => unityContainer.Resolve(type));

                    x.CreateMap<JsonPlayer, Player>().
                        ForMember(dest => dest.Position,
                                  opt => opt.ResolveUsing<PositionResolver>().FromMember(p => p.TypeName)).
                        ForMember(dest => dest.Status,
                                  opt => opt.ResolveUsing<PlayerStatusResolver>().FromMember(p => p.Status)).
                        ForMember(dest => dest.Name, opt => opt.ResolveUsing<PlayerNameResolver>()).
                        ForMember(dest => dest.ClubCode,
                                  opt => opt.ResolveUsing<TeamNameResolver>().FromMember(p => p.TeamName)).
                        ForMember(dest => dest.FutureFixtures,
                                  opt => opt.ResolveUsing<FutureFixturesResolver>().FromMember(p => p.Fixtures.All)).
                        ForMember(dest => dest.PastFixtures,
                                  opt => opt.ResolveUsing<PastFixturesResolver>().FromMember(p => p.FixtureHistory.All))
                        .
                        ForMember(dest => dest.PastSeasons,
                                  opt => opt.ResolveUsing<PastSeasonsResolver>().FromMember(p => p.SeasonHistory));
                });

            Mapper.CreateMap<PastFixture, FutureFixture>();

            
        }
    }
}
