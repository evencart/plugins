using DryIoc;
using Genesis.Infrastructure.DependencyManager;
using Ui.Slider.Services;

namespace Ui.Slider
{
    public class DependencyContainer : IPluginDependencyContainer
    {
        public void RegisterDependencies(IRegistrator registrar)
        {
            registrar.Register<IUiSliderService, UiSliderService>(Reuse.ScopedOrSingleton);
        }

        public void RegisterDependenciesIfActive(IRegistrator registrar)
        {
            
        }

        public int Priority { get; } = 0;
    }
}