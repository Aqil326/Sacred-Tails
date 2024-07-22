using PlayFab.ClientModels;
using Timba.Patterns.ServiceLocator;

namespace Timba.Games.SacredTails
{
    public interface IWallet : IService
    {
        void UpdateUserWallet();
        int GetUserCurrentCoins();
        void HideUserWallet();
        void ShowUserWallet();
    }
}