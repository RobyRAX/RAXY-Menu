using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace RAXY.Menu
{
    [Serializable]
    public class TabChangedEvent : UnityEvent<string>
    {
    }

    [Serializable]
    public class TabEntry
    {
        public string id;
        public Toggle toggle;
        [ListDrawerSettings(Expanded = true)]
        public List<GameObject> panels;

        [TitleGroup("Event")]
        public TabChangedEvent onTabOpened = new TabChangedEvent();
    }

    public class TabGroup : MonoBehaviour
    {
        /// <summary>
        /// parameters: string tabId
        /// </summary>
        public event Action<string> OnTabChanged;

        [SerializeField]
        [ValueDropdown(nameof(GetTabIdDropdown))]
        string defaultTabId;

        [ListDrawerSettings(Expanded = true, ShowIndexLabels = true, ListElementLabelName = "id")]
        [SerializeField]
        List<TabEntry> tabs = new List<TabEntry>();

        int _currentIndex = -1;
        bool _suppressToggleCallback;

        void Awake()
        {
            SetupToggles();
        }

        void OnEnable()
        {
            if (_currentIndex < 0)
                SelectDefault();
        }

        public void SelectTab(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i] != null && tabs[i].id == id)
                {
                    SelectTab(i);
                    return;
                }
            }
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= tabs.Count)
                return;

            ApplySelection(index);
        }

        public void SelectDefault()
        {
            var defaultIndex = GetDefaultIndex();
            if (defaultIndex >= 0)
                ApplySelection(defaultIndex);
        }

        void ApplySelection(int index)
        {
            _currentIndex = index;
            var selectedPanels = new HashSet<GameObject>();
            var selectedTab = tabs[index];
            if (selectedTab != null && selectedTab.panels != null)
            {
                foreach (var panel in selectedTab.panels)
                {
                    if (panel != null)
                        selectedPanels.Add(panel);
                }
            }

            _suppressToggleCallback = true;
            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                if (tab == null)
                    continue;

                var isActive = i == index;

                if (tab.toggle != null)
                {
                    if (tab.toggle.isOn != isActive)
                        tab.toggle.isOn = isActive;
                    tab.toggle.interactable = !isActive;
                }

                if (tab.panels != null)
                {
                    foreach (var panel in tab.panels)
                    {
                        if (panel != null)
                            panel.SetActive(selectedPanels.Contains(panel));
                    }
                }
            }
            _suppressToggleCallback = false;

            var selected = tabs[index];
            OnTabChanged?.Invoke(selected != null ? selected.id : null);
            if (selected != null)
                selected.onTabOpened?.Invoke(selected.id);
        }

        int GetDefaultIndex()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i] != null && tabs[i].id == defaultTabId)
                    return i;
            }

            return tabs.Count > 0 ? 0 : -1;
        }

        IEnumerable<string> GetTabIdDropdown()
        {
            if (tabs == null)
                yield break;

            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                if (tab != null && !string.IsNullOrEmpty(tab.id))
                    yield return tab.id;
            }
        }

        void SetupToggles()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                if (tab == null || tab.toggle == null)
                    continue;

                int index = i;
                tab.toggle.onValueChanged.RemoveAllListeners();
                tab.toggle.onValueChanged.AddListener(isOn =>
                {
                    if (_suppressToggleCallback)
                        return;
                    if (isOn)
                        SelectTab(index);
                });
            }
        }

#if UNITY_EDITOR
        public void EnsureTabOrder(IEnumerable<string> desiredIds)
        {
            if (desiredIds == null)
                return;

            var existing = new List<TabEntry>(tabs);
            var used = new HashSet<TabEntry>();
            var result = new List<TabEntry>();
            var usedIds = new HashSet<string>();

            foreach (var id in desiredIds)
            {
                if (string.IsNullOrEmpty(id))
                    continue;

                TabEntry match = null;
                for (int i = 0; i < existing.Count; i++)
                {
                    var tab = existing[i];
                    if (tab == null || used.Contains(tab))
                        continue;
                    if (tab.id == id)
                    {
                        match = tab;
                        break;
                    }
                }

                if (match == null)
                {
                    match = new TabEntry
                    {
                        id = id,
                        panels = null,
                        toggle = null,
                    };
                }
                else
                {
                    used.Add(match);
                }

                result.Add(match);
                usedIds.Add(id);
            }

            for (int i = 0; i < existing.Count; i++)
            {
                var tab = existing[i];
                if (tab == null)
                    continue;

                if (used.Contains(tab))
                    continue;

                if (string.IsNullOrEmpty(tab.id))
                {
                    result.Add(tab);
                    used.Add(tab);
                    continue;
                }

                if (!usedIds.Contains(tab.id))
                {
                    result.Add(tab);
                    used.Add(tab);
                }
                else
                {
                    // duplicate id, keep it to avoid data loss
                    result.Add(tab);
                    used.Add(tab);
                }
            }

            tabs = result;
        }
#endif
    }
}
