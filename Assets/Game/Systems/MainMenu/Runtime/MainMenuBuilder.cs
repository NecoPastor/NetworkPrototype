using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems.MainMenu.Runtime
{
    public class MenuBuilder : MonoBehaviour
    {
        [Header("UI References")]
        public Transform Container;
        public MenuItemView Prefab;
        public TextMeshProUGUI TitleText;
        public MenuConfig defaultMenuConfig;
        private MenuConfig currentConfig;

        private void Start()
        {
            BuildMenu(defaultMenuConfig);
        }

        public void BuildMenu(MenuConfig config)
        {
            currentConfig = config;

            // Очистка предыдущих элементов
            foreach (Transform child in Container)
                Destroy(child.gameObject);

            // Заголовок
            TitleText.text = config.Title;

            // Фильтрация и сортировка
            var items = config.Items
                .Where(i => i.Condition == null || i.Condition.IsMet());

            if (config.SortByCondition)
                items = items.OrderByDescending(i => i.Condition?.IsMet() ?? true);

            // Создание элементов
            foreach (var item in items)
            {
                var view = Instantiate(Prefab, Container);
                view.Initialize(item);
            }

            ApplySpacing(config.ItemSpacing);
        }

        private void ApplySpacing(float spacing)
        {
            var layout = Container.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
                layout.spacing = spacing;
        }
    }
}