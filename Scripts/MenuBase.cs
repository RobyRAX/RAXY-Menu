using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Menu
{
    public abstract class MenuBase : MonoBehaviour
    {
        public string MenuId { get; set; }
        public MenuManagerBase MenuManager { get; set; }

        [PropertyOrder(-2)]
        public BackButton backBtn;

        // Tambahkan parameter context opsional
        public virtual void Open(OpenMenuConfigBase config = null)
        {
            if (config != null)
            {
                MenuManager = config.MenuManager;
                backBtn.Setup(MenuManager);
            }

            gameObject.SetActive(true);
        }
        
        public virtual void Close()
        {
            gameObject.SetActive(false);
        }

        public virtual void Refresh()
        {
        }
    }

    public abstract class OpenMenuConfigBase
    {
        public MenuManagerBase MenuManager { get; set; }

        //public OpenMenuConfigBase() { }
        public OpenMenuConfigBase(MenuManagerBase menuManager)
        {
            MenuManager = menuManager;
        }
    }
}
