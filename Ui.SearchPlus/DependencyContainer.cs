using DryIoc;
using Genesis.Infrastructure.DependencyManager;
using Ui.SearchPlus.Factories;
using Ui.SearchPlus.Services;

namespace Ui.SearchPlus
{
    public class DependencyContainer : IPluginDependencyContainer
    {
        public void RegisterDependencies(IRegistrator registrar)
        {
            registrar.Register<ISearchTermService, SearchTermService>(Reuse.ScopedOrSingleton);
            registrar.Register<ISearchTermModelFactory, SearchTermModelFactory>();
        }

        public void RegisterDependenciesIfActive(IRegistrator registrar)
        {
            
        }

        public int Priority { get; } = 0;
    }
}