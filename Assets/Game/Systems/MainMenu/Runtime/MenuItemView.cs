using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems.MainMenu.Runtime
{
    public class MenuItemView : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI TitleText;
        public Image IconImage;
        public Button Button;

        private MenuItemConfig _config;

        public void Initialize(MenuItemConfig config)
        {
            _config = config;
            TitleText.text = config.Title;
            IconImage.sprite = config.Icon;

            Button.onClick.AddListener(() =>
            {
                config.Action?.Execute();
            });
        }
    }
}
