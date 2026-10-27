using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace RAXY.Menu
{
    public class BackButton : MonoBehaviour
    {
        [SerializeField]
        Button button;

        [ShowInInspector]
        MenuManagerBase menuManager; 

        public void Setup(MenuManagerBase menuManager)
        {
            this.menuManager = menuManager;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                menuManager.Back();
                //Debug.Log("test");
            });
        }
    }
}
