using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the navigation bar and coordinates the visibility of exclusive UI panels.
/// </summary>
public class HotbarUI : MonoBehaviour
{
    /// <summary>
    /// Configuration wrapper for a menu category.
    /// </summary>
    [Serializable]
    public struct MenuCategory
    {
        public string name;
        public Button button;
        public GameObject panel;
    }

    [Header("Seznam kategorií")]
    [SerializeField] private List<MenuCategory> menuCategories;

    private GameObject activePanel;

    private void Start()
    {
        CloseAllPanels();

        foreach (MenuCategory category in menuCategories)
        {
            GameObject panelToToggle = category.panel;

            category.button.onClick.AddListener(() => {
                TogglePanel(panelToToggle);
            });
        }
    }

    /// <summary>
    /// Toggles the active state of a specific panel. 
    /// </summary>
    /// <param name="panel">The target GameObject to toggle.</param>
    private void TogglePanel(GameObject panel)
    {
        if (activePanel == panel)
        {
            CloseAllPanels();
        }
        else
        {
            if (activePanel != null)
            {
                activePanel.SetActive(false);
            }

            panel.SetActive(true);
            activePanel = panel;
        }
    }

    /// <summary>
    /// Deactivates all registered panels and resets the building selection state.
    /// </summary>
    private void CloseAllPanels()
    {
        foreach (MenuCategory category in menuCategories)
        {
            if (category.panel != null)
            {
                category.panel.SetActive(false);
            }
        }
        activePanel = null;

        if (GridBuildingSystem.Instance != null)
        {
            GridBuildingSystem.Instance.DeselectObjectType();
        }
    }

    public void ForceCloseAll()
    {
        CloseAllPanels();
    }
}