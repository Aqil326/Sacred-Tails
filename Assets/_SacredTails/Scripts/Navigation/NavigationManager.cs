using UnityEngine.SceneManagement;
using UnityEngine;

namespace Timba.SacredTails.Navigation
{
    public class NavigationManager : MonoBehaviour, INavigation
    {

        public void GoToScene(int index)
        {
            SceneManager.LoadScene(index);
        }


        public bool IsReady()
        {
            return true;
        }
    }
}

