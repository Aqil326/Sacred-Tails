using UnityEngine;
using UnityEngine.UI;
using Timba.SacredTails.Navigation;
using Timba.Patterns.ServiceLocator;

public class DummyMainMenuNavigation : MonoBehaviour
{
    [SerializeField] private Button lobbyBtn;
    [SerializeField] private Button characterToolBtn;
    INavigation navigation;

    private void Awake()
    {
        navigation = ServiceLocator.Instance.GetService<INavigation>();

        lobbyBtn.onClick.AddListener(()=> navigation.GoToScene(1));
        characterToolBtn.onClick.AddListener(() => navigation.GoToScene(2));
    }
}
