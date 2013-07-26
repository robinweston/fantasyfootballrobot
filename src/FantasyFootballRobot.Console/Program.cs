using System;
using FantasyFootballRobot.Core.DependencyInjection;
using FantasyFootballRobot.Core.Logging;
using FantasyFootballRobot.Core.Mapping;
using FantasyFootballRobot.Core.Simulators;
using Microsoft.Practices.Unity;

namespace FantasyFootballRobot.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (IUnityContainer container = ContainerBuilder.BuildUnityContainer())
            {
                MappingConfiguration.Bootstrap(container);
                
                var logger = container.Resolve<ILogger>();

                var arguments = new Arguments(args);
                var simulationOptions = CreateSimulationOptions(arguments);

                try
                {
                    SimulationRunner.RunSimulations(simulationOptions, container, logger);
                }
                catch (Exception ex)
                {
                    logger.Log(Tag.Error, ex.GetType().Name + " " + ex.Message);
                    logger.Log(Tag.Error, ex.StackTrace);

                    if (ex.InnerException != null)
                    {
                        logger.Log(Tag.Error, ex.InnerException.GetType().Name + " " + ex.Message);
                        logger.Log(Tag.Error, ex.InnerException.StackTrace);
                    }

                    logger.Log(Tag.Error, "Exiting due to error");
                }
            }
        }

        private static SeasonSimulationOptions CreateSimulationOptions(Arguments arguments)
        {
            var options = new SeasonSimulationOptions
            {
                CalculateTeamStrengthOnly = GetBoolArgument(arguments, ConsoleOptions.CalculateTeamStrengthsOnly),
                ChooseInitialTeamOnly = GetBoolArgument(arguments, ConsoleOptions.ChooseInitialTeamOnly),
                CalculatePlayerFormOnly = GetBoolArgument(arguments, ConsoleOptions.CalculatePlayerFormOnly),
                ListUsage = GetBoolArgument(arguments, ConsoleOptions.ListUsage),
                CalculateHomeAdvantageRatioOnly = GetBoolArgument(arguments, ConsoleOptions.CalculateHomeAdvantageOnly),
                UseSavedInitialTeam = GetBoolArgument(arguments, ConsoleOptions.UseSavedInitialTeam),
                StrategyName = GetStringArgument(arguments, ConsoleOptions.StrategyName)
            };

            var maxGameweekString = arguments[ConsoleOptions.MaxGameweek];
            if (!string.IsNullOrWhiteSpace(maxGameweekString))
            {
                options.MaximumGameweek = int.Parse(maxGameweekString);
            }

            return options;
        }

        private static string GetStringArgument(Arguments arguments, string argumentName)
        {
            return arguments[argumentName];
        }

        private static bool GetBoolArgument(Arguments arguments, string argumentName)
        {
            return string.Equals(arguments[argumentName], bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
        }

        
    }
}