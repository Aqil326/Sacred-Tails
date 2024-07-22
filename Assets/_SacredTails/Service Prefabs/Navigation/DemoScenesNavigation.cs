using UnityEngine;
using UnityEngine.UI;
using Timba.SacredTails.Navigation;
using Timba.Games.CharacterFactory;
using Timba.Patterns.ServiceLocator;

public class DemoScenesNavigation : MonoBehaviour
{
    [SerializeField] private Button backBtn;
    INavigation navigation;

    private void Awake()
    {
        navigation = ServiceLocator.Instance.GetService<INavigation>();
        ColorSwapper3D.Instance.isCharacterViewScene = true;
        backBtn.onClick.AddListener(() => navigation.GoToScene(0));
    }
}
