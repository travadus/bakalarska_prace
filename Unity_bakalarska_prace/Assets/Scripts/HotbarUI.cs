using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarUI : MonoBehaviour
{
    // Definujeme si strukturu pro jednu kategorii (napø. Buildings)
    // [Serializable] zajistí, že to uvidíme v Inspectoru a mùžeme to editovat
    [Serializable]
    public struct MenuCategory
    {
        public string name;        // Jen pro pøehlednost v editoru (napø. "Budovy")
        public Button button;      // Tlaèítko na Hotbaru (napø. "Buildings Button")
        public GameObject panel;   // Panel, který se má otevøít (napø. "Buildings Panel")
    }

    [Header("Seznam kategorií")]
    [SerializeField] private List<MenuCategory> menuCategories;

    // Ukládáme si referenci na právì otevøený panel, abychom vìdìli, co zavøít
    private GameObject activePanel;

    private void Start()
    {
        // 1. Ujistíme se, že na zaèátku jsou všechny panely zavøené
        CloseAllPanels();

        // 2. Projdeme všechny kategorie a nastavíme tlaèítkùm funkci
        foreach (MenuCategory category in menuCategories)
        {
            // Musíme si uložit lokální kopii promìnné pro Lambda výraz (aby to fungovalo správnì v cyklu)
            GameObject panelToToggle = category.panel;

            category.button.onClick.AddListener(() => {
                TogglePanel(panelToToggle);
            });
        }
    }

    private void TogglePanel(GameObject panel)
    {
        // SCÉNÁØ A: Klikli jsme na tlaèítko panelu, který je už otevøený -> Zavøít ho
        if (activePanel == panel)
        {
            CloseAllPanels();
        }
        // SCÉNÁØ B: Klikli jsme na jiný panel (nebo žádný nebyl otevøený) -> Otevøít nový
        else
        {
            // Nejdøív zavøeme ten starý (pokud nìjaký je)
            if (activePanel != null)
            {
                activePanel.SetActive(false);
            }

            // Otevøeme ten nový
            panel.SetActive(true);
            activePanel = panel;
        }
    }

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

        // VOLITELNÉ: Pokud zavøeme všechny panely UI, možná chceme zrušit i výbìr budovy v ruce?
        GridBuildingSystem.Instance.DeselectObjectType();
    }

    // Veøejná metoda, kdybys ji potøeboval volat odjinud (napø. pøi stisku ESC)
    public void ForceCloseAll()
    {
        CloseAllPanels();
    }
}