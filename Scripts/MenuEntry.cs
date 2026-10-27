using System;
using RAXY.Core.Addressable;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RAXY.Menu
{
    [Serializable]
    public class MenuEntry : IAddressableAssetProvider<GameObject>
    {
        public string menuId;

        [SerializeField]
        bool useAddressable;
        public bool UseAddressable => useAddressable;

        [HideIf("UseAddressable")]
        public bool isPrefab;

        public bool isOverlay;

        [HideIf("UseAddressable")]
        [SerializeField]
        GameObject directAsset;
        public GameObject DirectAsset => directAsset;

        [ShowIf("UseAddressable")]
        [SerializeField]
        AssetReferenceT<GameObject> assetReference;

        public AssetReferenceT<GameObject> AssetReference => assetReference;

        public GameObject CachedAddressableAsset { get; set; }

        public MenuBase Menu
        {
            get
            {
                if (useAddressable)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        var editorAsset = AssetReference?.editorAsset as GameObject;
                        return editorAsset ? editorAsset.GetComponent<MenuBase>() : null;
                    }
#endif

                    if (!CachedAddressableAsset)
                    {
                        CachedAddressableAsset = AddressableService.GetLoadedAsset<GameObject>(AssetReference);

                        if (!CachedAddressableAsset)
                        {
                            Debug.LogWarning($"[GameObject] Addressable asset NOT loaded yet for {AssetReference?.AssetGUID}");
                        }
                    }

                    return CachedAddressableAsset ? CachedAddressableAsset.GetComponent<MenuBase>() : null;
                }
                else
                {
                    return directAsset ? directAsset.GetComponent<MenuBase>() : null;
                }
            }
            set
            {
                if (useAddressable)
                {
                    CachedAddressableAsset = value?.gameObject;
                }
                else
                {
                    directAsset = value?.gameObject;
                }
            }
        }

        public void SetResolvedInstance(GameObject instance)
        {
            if (useAddressable)
                CachedAddressableAsset = instance;
            else
                directAsset = instance;
        }
    }
}
