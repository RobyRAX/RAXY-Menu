using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using RAXY.Utility;
using RAXY.Core.Addressable;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace RAXY.Menu
{
    public abstract class MenuManagerBase : MonoBehaviour
    {
        [TitleGroup("Menu Entry")]
        public Transform menuRoot;

        [TitleGroup("Menu Entry")]
        [SerializeField]
        bool openMainMenuOnAwake;

        [TitleGroup("Menu Entry")]
        [InlineButton("RefreshMenuIds", "Refresh")]
        [ValueDropdown("MenuIds")]
        public string mainMenuId;

#if UNITY_EDITOR
        static List<string> MenuIds;

        void RefreshMenuIds()
        {
            MenuIds = new();
            MenuIds = menuEntries.Select(x => x.menuId).ToList();
        }
#endif

        [TitleGroup("Menu Entry")]
        [ListDrawerSettings(ListElementLabelName = "menuId", Expanded = true)]
        public List<MenuEntry> menuEntries;

        [TitleGroup("Menu Instance")]
        [ShowInInspector]
        [HideInEditorMode]
        [DictionaryDrawerSettings(KeyLabel = "Menu Id", ValueLabel = "Menu Runtime Handle")]
        [HideReferenceObjectPicker]
        readonly Dictionary<string, MenuRuntimeHandle> menuInstanceMap = new Dictionary<string, MenuRuntimeHandle>();

        [TitleGroup("Stack")]
        [ShowInInspector]
        [HideInEditorMode]
        [HideReferenceObjectPicker]
        public Stack<MenuRuntimeHandle> menuStack = new Stack<MenuRuntimeHandle>();

        protected void Awake()
        {
            foreach (var provider in menuEntries)
            {
                if (provider.UseAddressable == false)
                {
                    if (provider.isPrefab == false)
                    {
                        var directHandle = BuildHandleFromInstance(provider.DirectAsset, provider);
                        if (directHandle != null)
                        {
                            provider.SetResolvedInstance(directHandle.TargetGameObject);
                            if (!string.IsNullOrEmpty(provider.menuId))
                                menuInstanceMap[provider.menuId] = directHandle;
                            directHandle.Close();
                        }
                    }
                }
            }

            TryOpenMainMenuAsync().Forget();
        }

        [TitleGroup("Debug Function")]
        [Button]
        public virtual async UniTask OpenMenu(string menuId, OpenMenuConfigBase config = null)
        {
            var menuEntry = await GetMenuProviderAsync(menuId);
            if (menuEntry == null)
                return;

            var menu = await ResolveMenuAsync(menuEntry);

            if (menu == null) 
                return;

            var isOverlay = menuEntry.isOverlay;

            if (!isOverlay && menuStack.Count == 0 && !string.IsNullOrEmpty(mainMenuId))
            {
                if (menuInstanceMap.TryGetValue(mainMenuId, out var mainMenu) && mainMenu)
                {
                    if (mainMenu != menu && mainMenu.IsActive)
                        mainMenu.Close();
                }
            }

            // Tutup/Sembunyikan menu saat ini terlebih dahulu jika ada
            if (menuStack.Count > 0)
            {
                var currentMenu = menuStack.Peek();
                if (currentMenu == menu || (!string.IsNullOrEmpty(currentMenu.MenuId) && currentMenu.MenuId == menu.MenuId))
                    return;

                if (!isOverlay)
                    currentMenu.Close();
            }

            // Masukkan menu baru ke dalam tumpukan stack, lalu buka
            menuStack.Push(menu);
            menu.Open(config);
        }

        public async UniTask<MenuEntry> GetMenuProviderAsync(string menuId)
        {
            if (string.IsNullOrEmpty(menuId))
                return null;
            
            return menuEntries.Find(x => x.menuId == menuId);
        }

        [TitleGroup("Debug Function")]
        [Button]
        public virtual void Back()
        {
            if (menuStack.Count > 0)
            {
                // Hapus menu paling atas dari stack dan tutup
                var currentMenu = menuStack.Pop();
                currentMenu.Close();
            }

            // Buka kembali menu yang berada di bawahnya (sebelumnya)
            if (menuStack.Count > 0)
            {
                var previousMenu = menuStack.Peek();
                if (!previousMenu.IsActive)
                    previousMenu.Open(); 
                previousMenu.Refresh();
            }
            else
            {
                TryOpenMainMenuAsync().Forget();
            }
        }

        [TitleGroup("Debug Function")]
        [Button]
        public virtual void CloseAll()
        {
            while (menuStack.Count > 0)
            {
                var menu = menuStack.Pop();
                menu.Close();
            }

            TryOpenMainMenuAsync().Forget();
        }

        [TitleGroup("Debug Function")]
        [Button]
        public virtual void RefreshCurrentMenu()
        {
            if (menuStack.Count > 0)
            {
                menuStack.Peek().Refresh();
                return;
            }

            if (string.IsNullOrEmpty(mainMenuId))
                return;

            if (!menuInstanceMap.TryGetValue(mainMenuId, out var mainMenu))
                return;

            if (mainMenu.IsActive)
                mainMenu.Refresh();
        }

        async UniTask<MenuRuntimeHandle> ResolveMenuAsync(MenuEntry provider)
        {
            if (provider == null)
                return null;

            if (!string.IsNullOrEmpty(provider.menuId) && menuInstanceMap.TryGetValue(provider.menuId, out var cachedMenu))
                return cachedMenu;

            var root = menuRoot ? menuRoot : transform;

            if (provider.UseAddressable)
            {
                var prefab = await AddressableService.ResolveAsync(provider);
                if (!prefab)
                {
                    Debug.LogError($"[MenuManager] Failed to load addressable menu for {provider.menuId}");
                    return null;
                }

                var instanceGo = Instantiate(prefab, root);
                var instanceHandle = BuildHandleFromInstance(instanceGo, provider);
                if (instanceHandle == null)
                    return null;

                if (!string.IsNullOrEmpty(provider.menuId))
                    menuInstanceMap[provider.menuId] = instanceHandle;
                provider.SetResolvedInstance(instanceHandle.TargetGameObject);
                return instanceHandle;
            }

            if (!provider.isPrefab)
            {
                var directAsset = provider.DirectAsset;
                if (!directAsset)
                {
                    Debug.LogError($"[MenuManager] Direct menu is null for {provider.menuId}");
                    return null;
                }

                var directHandle = BuildHandleFromInstance(directAsset, provider);
                if (directHandle == null)
                    return null;

                if (!string.IsNullOrEmpty(provider.menuId))
                    menuInstanceMap[provider.menuId] = directHandle;
                provider.SetResolvedInstance(directHandle.TargetGameObject);
                return directHandle;
            }

            var directPrefab = provider.DirectAsset;
            if (!directPrefab)
            {
                Debug.LogError($"[MenuManager] Direct menu prefab is null for {provider.menuId}");
                return null;
            }

            var directInstanceGo = Instantiate(directPrefab, root);
            var directInstanceHandle = BuildHandleFromInstance(directInstanceGo, provider);
            if (directInstanceHandle == null)
                return null;

            if (!string.IsNullOrEmpty(provider.menuId))
                menuInstanceMap[provider.menuId] = directInstanceHandle;
            provider.SetResolvedInstance(directInstanceHandle.TargetGameObject);
            return directInstanceHandle;
        }

        async UniTask TryOpenMainMenuAsync()
        {
            if (openMainMenuOnAwake == false)
                return;

            if (string.IsNullOrEmpty(mainMenuId))
                return;

            var provider = await GetMenuProviderAsync(mainMenuId);
            if (provider == null)
                return;

            var menu = await ResolveMenuAsync(provider);
            if (!menu)
                return;

            if (!menu.IsActive)
                menu.Open();
        }

        MenuRuntimeHandle BuildHandleFromInstance(GameObject menuGo, MenuEntry entry)
        {
            if (!menuGo)
            {
                Debug.LogError($"[MenuManager] Failed to build menu handle because target GameObject is null for {entry?.menuId}");
                return null;
            }

            var menuBase = menuGo.GetComponent<MenuBase>();
            if (menuBase != null)
                menuBase.MenuId = entry.menuId;

            return new MenuRuntimeHandle(entry.menuId, menuBase, menuGo, entry);
        }
    }

    [HideReferenceObjectPicker]
    public class MenuRuntimeHandle
    {
        [ShowInInspector]
        public string MenuId { get; }
        [ShowInInspector]
        public MenuBase MenuBase { get; }
        [ShowInInspector]
        public GameObject TargetGameObject { get; }

        public bool IsActive => TargetGameObject && TargetGameObject.activeSelf;

        public MenuRuntimeHandle(string menuId, MenuBase menuBase, GameObject targetGameObject, MenuEntry entry)
        {
            MenuId = menuId;
            MenuBase = menuBase;
            TargetGameObject = targetGameObject;
        }

        public void Open(OpenMenuConfigBase context = null)
        {
            if (!TargetGameObject)
                return;

            if (MenuBase != null)
            {
                MenuBase.Open(context);
                return;
            }

            TargetGameObject.SetActive(true);
        }

        public void Close()
        {
            if (!TargetGameObject)
                return;

            if (MenuBase != null)
            {
                MenuBase.Close();
                return;
            }

            TargetGameObject.SetActive(false);
        }

        public void Refresh()
        {
            if (!TargetGameObject)
                return;

            if (MenuBase != null)
                MenuBase.Refresh();
        }

        public static implicit operator bool(MenuRuntimeHandle handle)
        {
            return handle != null && handle.TargetGameObject;
        }
    }
}
