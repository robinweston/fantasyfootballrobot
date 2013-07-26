using System;
using System.Collections.Generic;
using FantasyFootballRobot.Core.Configuration;
using FantasyFootballRobot.Core.Entities;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Services;
using FantasyFootballRobot.Core.Simulators;
using FantasyFootballRobot.Core.Strategies;
using FantasyFootballRobot.Core.Strategies.Complex;
using FantasyFootballRobot.Core.Strategies.Complex.Parameters;
using FantasyFootballRobot.Core.Strategies.Complex.Prediction;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.Gameweek;
using FantasyFootballRobot.Core.Strategies.Complex.Selection.SeasonStart;
using FantasyFootballRobot.Core.Strategies.Complex.Transfers;
using FantasyFootballRobot.Core.Strategies.Genetic;
using Microsoft.Practices.Unity;

namespace FantasyFootballRobot.Core.DependencyInjection
{
    public static class ContainerBuilder
    {
        private const string LocalPlayerJsonServiceName = "LocalPlayerJsonService";
        private const string RemotePlayerJsonServiceName = "RemotePlayerJsonServiceName";

        public static IUnityContainer BuildUnityContainer()
        {
            IUnityContainer container = new UnityContainer()
                .RegisterType<IConfigurationSettings, ConfigurationSettings>();

            RegisterLoggers(container);
            RegisterSeasonSimulatorTypes(container);
            RegisterPlayerServices(container);
            RegisterStrategy(container);

            return container;
        }

        private static void RegisterLoggers(IUnityContainer container)
        {
            container.RegisterType<ILogger, ConsoleLogger>(
                  new InjectionFactory(c => new ConsoleLogger(new TextFileLogger(new ImportantTextFileLogger(new CsvLogger())))));
        }

        private static void RegisterStrategy(IUnityContainer container)
        {
            container.RegisterType<IInitialTeamSelectorStrategy, InitialTeamSelectorStrategy>()
                .RegisterType<ITransferValidator, TransferValidator>()
                .RegisterType<ITransferActioner, TransferActioner>()

                .RegisterType<IGeneticAlgorithm<FitTeam, IList<Player>>, GeneticTeamSelector>()

                .RegisterType<IGeneticAlgorithm<FitTransferActions, SeasonState>, GeneticTransferSelector>()

                .RegisterType<ITeamScorePredictor, TeamScorePredictor>()

                .RegisterType<ITransferSelectorStrategy, TransferSelectorStrategy>()

                .RegisterType<IPlayerScorePredictor, PlayerScorePredictor>()
                .RegisterType<ITeamGameweekSelector, TeamGameweekSelector>()

                .RegisterType<ITeamStrengthCalculator, TeamStrengthCalculator>()
                .RegisterType<IPlayerPoolReducer, PlayerPoolReducer>()

                .RegisterType<IPlayerFormCalculator, PlayerFormCalculator>()
                .RegisterType<ILocationAdvantageCalculator, LocationAdvantageCalculator>()
                .RegisterType<IRandom, RandomWrapper>();

            RegisterStrategyParameters(container);
        }

        private static void RegisterStrategyParameters(IUnityContainer container)
        {
            container.RegisterType<IStrategy, ComplexStrategy>()
                .RegisterType<IGeneticParameters, InitialTeamSelectionParameters>()
                .RegisterType<IInitialTeamSelectionParameters, InitialTeamSelectionParameters>()
                .RegisterType<IPredictorParameters, PredictorParameters>();
        }

        private static void RegisterSeasonSimulatorTypes(IUnityContainer container)
        {
              container.RegisterType<IGameweekSimulator, GameweekSimulator>()
                .RegisterType<ISeasonSimulator, SeasonSimulator>()
                .RegisterType<ITimeAdjustor, TimeAdjustor>()
                .RegisterType<IDecisionActioner, DecisionActioner>();
        }

        private static void RegisterPlayerServices(IUnityContainer container)
        {
            //remote json service defined by name
            container.RegisterType<IPlayerJsonService, RemotePlayerJsonService>(RemotePlayerJsonServiceName);

            //local json service also defined by name, and takes a remote service in the constructor
            container.RegisterType<IPlayerJsonService, LocalPlayerJsonService>(LocalPlayerJsonServiceName, 
                new InjectionFactory(c => 
                    new LocalPlayerJsonService(
                        c.Resolve<IConfigurationSettings>(),
                        c.Resolve<IPlayerJsonService>(RemotePlayerJsonServiceName), c.Resolve<ILogger>())));

            //json parsing player service takes local service in constructor
            container.RegisterType<ISinglePlayerService, JsonParsingSinglePlayerService>(
                new InjectionFactory(
                    c =>
                    new JsonParsingSinglePlayerService(
                        c.Resolve<IPlayerJsonService>(LocalPlayerJsonServiceName),
                        c.Resolve<ILogger>()
                    )));

            //memory cacheable player service takes json parsing service in constructor
            container.RegisterType<IMultiplePlayersService, PlayerService>();

        }

    }
}