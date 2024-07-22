
using Timba.Patterns.ServiceLocator;

namespace Timba.SacredTails.Navigation
{
    public interface INavigation : IService
    {
        public void GoToScene(int index);
    }
}

