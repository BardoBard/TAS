using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Thor.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lithium.Core.Thor.Core
{
    public class TasPopupService : TasService, ITasPopupService
    {
        public string Name => "TasPopupService";
        public float LoadProgress => 1f;
        private bool m_isInitialized = false;
        private bool m_isShowing = false;

        private Dictionary<GameObject, List<Component>> m_panelElements = new Dictionary<GameObject, List<Component>>();

        /// <summary>
        /// For tracking panel size changes, and updating accordingly.
        /// </summary>
        private Vector2 m_lastPanelSize = Vector2.zero;
        public GlobalLayoutSettings GlobalElementSettings {get; private set;} = new GlobalLayoutSettings
        {
            Padding = new Vector2(10, 10),
            ElementSize = new Vector2(250, 50),
            ElementBackgroundColor = Color.white,
            ElementTextColor = Color.black,
            MaxElementsPerRow = 99,
            MaxFontSize = 24
        };

        private void AddTitleBar(GameObject panelObj, string panelName, PanelSettings settings)
        {
            GameObject titleBarObj = new GameObject("TitleBar", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            titleBarObj.transform.SetParent(panelObj.transform, false);

            RectTransform titleBarRect = titleBarObj.GetComponent<RectTransform>();
            titleBarRect.anchorMin = new Vector2(0, 1);
            titleBarRect.anchorMax = new Vector2(1, 1);
            titleBarRect.pivot = new Vector2(0.5f, 0);
            titleBarRect.anchoredPosition = new Vector2(0, 0);
            titleBarRect.sizeDelta = new Vector2(0, settings.TitleHeight);

            var barImage = titleBarObj.GetComponent<Image>();
            barImage.color = settings.TitleBarColor;

            GameObject titleTextObj = new GameObject("Title", typeof(RectTransform));
            titleTextObj.transform.SetParent(titleBarObj.transform, false);

            RectTransform titleTextRect = titleTextObj.GetComponent<RectTransform>();
            titleTextRect.anchorMin = Vector2.zero;
            titleTextRect.anchorMax = Vector2.one;
            titleTextRect.offsetMin = Vector2.zero;
            titleTextRect.offsetMax = Vector2.zero;

            var titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleText.text = panelName;
            titleText.fontSize = GlobalElementSettings.MaxFontSize + 4;
            titleText.color = settings.TitleTextColor;
            titleText.alignment = TMPro.TextAlignmentOptions.Center;
        }

        private bool CreateCanvas(PanelSettings settings, out GameObject panelObj)
        {
            panelObj = null;
            Canvas mainCanvas = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .FirstOrDefault(c => c.gameObject.name == "UI Canvas");

            if (mainCanvas == null)
            {
                TasServices.Log.Log($"[{Name}]: Could not find UI Canvas");
                return false;
            }

            panelObj = new GameObject(settings.Name, typeof(RectTransform), typeof(CanvasGroup));
            if (panelObj == null)
                return false;

            panelObj.transform.SetParent(mainCanvas.transform, false);

            // Draggable Panel
            panelObj.AddComponent<DraggablePanel>();
            var draggablePanel = panelObj.GetComponent<DraggablePanel>();
            draggablePanel.Init(settings);

            // Resize Handle
            GameObject resizeHandleObj =
                new GameObject("ResizeHandle", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            resizeHandleObj.transform.SetParent(panelObj.transform, false);
            RectTransform resizeHandleRect = resizeHandleObj.GetComponent<RectTransform>();
            resizeHandleRect.anchorMin = new Vector2(1, 0);
            resizeHandleRect.anchorMax = new Vector2(1, 0);
            resizeHandleRect.pivot = new Vector2(1, 0);
            resizeHandleRect.anchoredPosition = new Vector2(-10, 10);
            resizeHandleRect.sizeDelta = new Vector2(20, 20);
            var resizeHandleImage = resizeHandleObj.GetComponent<Image>();
            resizeHandleImage.color = Color.gray;
            var resizeHandle = resizeHandleObj.AddComponent<DraggableResizeHandle>();
            resizeHandle.Init(panelObj.GetComponent<RectTransform>(), settings);
            
            // Title Bar
            AddTitleBar(panelObj, settings.Name, settings);
            
            // Panel Background
            var image = panelObj.AddComponent<Image>();
            image.color = settings.BackgroundColor;

            RectTransform tasPanelRect = panelObj.GetComponent<RectTransform>();
            if (tasPanelRect == null)
                return false;

            tasPanelRect.anchorMin = new Vector2(0, 1);
            tasPanelRect.anchorMax = new Vector2(0, 1);
            tasPanelRect.pivot = new Vector2(0, 1);
            tasPanelRect.anchoredPosition = settings.StartingPosition;
            tasPanelRect.sizeDelta = new Vector2(settings.StartingSize.x, settings.StartingSize.y);

            // Offset due to title bar
            tasPanelRect.anchoredPosition -= new Vector2(0, settings.TitleHeight);
            tasPanelRect.sizeDelta += new Vector2(0, settings.TitleHeight);

            panelObj.SetActive(false);

            m_panelElements.Add(panelObj, new List<Component>());
            return true;
        }

        private void UpdateElementSizes(GameObject panelObj, RectTransform elementRect, int index)
        {
            int elementsPerRow = Mathf.Max(1, Mathf.Min(GlobalElementSettings.MaxElementsPerRow,
                (int)(panelObj.GetComponent<RectTransform>().rect.width / (int)(GlobalElementSettings.ElementSize.x + GlobalElementSettings.Padding.x))));
            int currentElementCount = index;
            float mNextElementX = GlobalElementSettings.Padding.x + (currentElementCount % elementsPerRow) * (GlobalElementSettings.ElementSize.x + GlobalElementSettings.Padding.x);
            float mNextElementY = -GlobalElementSettings.Padding.y - (int)(currentElementCount / elementsPerRow) * (GlobalElementSettings.ElementSize.y + GlobalElementSettings.Padding.y);
            elementRect.sizeDelta = GlobalElementSettings.ElementSize;
            
            elementRect.anchorMin = new Vector2(0, 1);
            elementRect.anchorMax = new Vector2(0, 1);
            elementRect.pivot = new Vector2(0, 1);
            elementRect.anchoredPosition = new Vector2(mNextElementX, mNextElementY);
        }

        private bool AddTextElement(GameObject panelObj, TextElementSettings settings)
        {
            if (panelObj == null)
                return false;

            GameObject textObj = new GameObject(settings.Name, typeof(RectTransform));
            textObj.transform.SetParent(panelObj.transform, false);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            if (textRect == null)
                return false;
            
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(0, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            UpdateElementSizes(panelObj, textRect, m_panelElements[panelObj].Count);

            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = settings.Text;
            tmpText.fontSize = settings.FontSize > 0 ? settings.FontSize : GlobalElementSettings.MaxFontSize;
            tmpText.color = settings.TextColor != default ? settings.TextColor : GlobalElementSettings.ElementTextColor;
            tmpText.alignment = TMPro.TextAlignmentOptions.Center;
            tmpText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            tmpText.overflowMode = TMPro.TextOverflowModes.Overflow;
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 10;
            tmpText.fontSizeMax = settings.FontSize > 0 ? settings.FontSize : GlobalElementSettings.MaxFontSize;
            tmpText.name = settings.Name;
            m_panelElements[panelObj].Add(tmpText);
            return true;
        }
                
        private bool AddDropdown(GameObject panelObj, DropdownSettings settings)
        {
            if (panelObj == null)
                return false;

            GameObject dropdownObj = new GameObject(settings.Name, typeof(RectTransform),
                typeof(CanvasGroup), typeof(Image));
            dropdownObj.transform.SetParent(panelObj.transform, false);

            RectTransform dropdownRect = dropdownObj.GetComponent<RectTransform>();
            if (dropdownRect == null)
                return false;
            UpdateElementSizes(panelObj, dropdownRect, m_panelElements[panelObj].Count);

            var dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
            var image = dropdownObj.GetComponent<Image>();
            image.color = GlobalElementSettings.ElementBackgroundColor;

            // Label
            GameObject labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(dropdownObj.transform, false);
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = settings.Options.Count > 0 ? settings.Options[0] : "";
            labelText.fontSize = GlobalElementSettings.MaxFontSize;
            labelText.color = GlobalElementSettings.ElementTextColor;
            labelText.alignment = TMPro.TextAlignmentOptions.Center;
            labelText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            labelText.overflowMode = TMPro.TextOverflowModes.Overflow;
            labelText.enableAutoSizing = true;
            labelText.fontSizeMin = 10;
            labelText.fontSizeMax = GlobalElementSettings.MaxFontSize;
            dropdown.captionText = labelText;

            // Template
            GameObject templateObj = new GameObject("Template", typeof(RectTransform), typeof(CanvasGroup),
                typeof(Image), typeof(ScrollRect));
            templateObj.transform.SetParent(dropdownObj.transform, false);
            templateObj.SetActive(false);
            RectTransform templateRect = templateObj.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1);

            int visibleItems = Mathf.Min(settings.MaxVisibleOptions, settings.Options.Count);
            templateRect.sizeDelta =
                new Vector2(0, GlobalElementSettings.ElementSize.y * visibleItems);
            var templateImage = templateObj.GetComponent<Image>();
            templateImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);

            // Viewport
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewportObj.transform.SetParent(templateObj.transform, false);
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            var viewportImage = viewportObj.GetComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.1f);
            viewportObj.GetComponent<Mask>().showMaskGraphic = false;

            // Content (Item Container)
            GameObject contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;

            // Scrollbar
            GameObject scrollbarObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(CanvasGroup),
                typeof(Image), typeof(Scrollbar));
            scrollbarObj.transform.SetParent(templateObj.transform, false);
            RectTransform scrollbarRect = scrollbarObj.GetComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1, 0);
            scrollbarRect.anchorMax = new Vector2(1, 1);
            scrollbarRect.pivot = new Vector2(1, 1);
            scrollbarRect.sizeDelta = new Vector2(20, 0);
            scrollbarRect.anchoredPosition = Vector2.zero;
            var scrollbarImage = scrollbarObj.GetComponent<Image>();
            scrollbarImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            // Scrollbar Handle
            GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObj.transform.SetParent(scrollbarObj.transform, false);
            RectTransform handleRect = handleObj.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0, 0);
            handleRect.anchorMax = new Vector2(1, 1);
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
            var handleImage = handleObj.GetComponent<Image>();
            handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            var scrollbar = scrollbarObj.GetComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.handleRect = handleRect;
            var scrollRect = templateObj.GetComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            // Item (Option)
            GameObject itemObj = new GameObject("Item", typeof(RectTransform), typeof(UnityEngine.UI.Toggle),
                typeof(Image));
            itemObj.transform.SetParent(contentObj.transform, false);
            RectTransform itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 1);
            itemRect.anchorMax = new Vector2(1, 1);
            itemRect.pivot = new Vector2(0.5f, 1);
            itemRect.sizeDelta = new Vector2(0, GlobalElementSettings.ElementSize.y);
            var itemImage = itemObj.GetComponent<Image>();
            itemImage.color = Color.white;

            // Item Label
            GameObject itemLabelObj = new GameObject("Item Label", typeof(RectTransform));
            itemLabelObj.transform.SetParent(itemObj.transform, false);
            RectTransform itemLabelRect = itemLabelObj.GetComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = Vector2.zero;
            itemLabelRect.offsetMax = Vector2.zero;
            var itemLabelText = itemLabelObj.AddComponent<TextMeshProUGUI>();
            itemLabelText.text = "";
            itemLabelText.fontSize = GlobalElementSettings.MaxFontSize;
            itemLabelText.color = GlobalElementSettings.ElementTextColor;
            itemLabelText.alignment = TMPro.TextAlignmentOptions.Center;
            itemLabelText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            itemLabelText.overflowMode = TMPro.TextOverflowModes.Overflow;
            itemLabelText.enableAutoSizing = true;
            itemLabelText.fontSizeMin = 10;
            itemLabelText.fontSizeMax = GlobalElementSettings.MaxFontSize;

            dropdown.template = templateRect;
            dropdown.captionText = labelText;
            dropdown.itemText = itemLabelText;

            dropdown.options.Clear();
            foreach (var opt in settings.Options) dropdown.options.Add(new TMP_Dropdown.OptionData(opt));
            dropdown.value = settings.DefaultIndex;

            dropdown.onValueChanged.AddListener((i) => settings.OnValueChanged?.Invoke(dropdown));

            m_panelElements[panelObj].Add(dropdown);

            return true;
        }
        
        private bool AddInputField(GameObject panelObj, InputFieldSettings settings)
        {
            if (panelObj == null)
                return false;
            
            GameObject inputFieldObj = new GameObject(settings.Name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            inputFieldObj.transform.SetParent(panelObj.transform, false);
            RectTransform inputFieldRect = inputFieldObj.GetComponent<RectTransform>();
            if (inputFieldRect == null)
                return false;
            
            UpdateElementSizes(panelObj, inputFieldRect, m_panelElements[panelObj].Count);
            var inputField = inputFieldObj.AddComponent<TMP_InputField>();
            if (inputField == null)
                return false;
            
            // Placeholder Text
            GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform));
            placeholderObj.transform.SetParent(inputFieldObj.transform, false);
            
            RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = settings.Placeholder;
            placeholderText.fontSize = GlobalElementSettings.MaxFontSize;
            placeholderText.color = Color.gray;
            placeholderText.alignment = TMPro.TextAlignmentOptions.Center;
            placeholderText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            placeholderText.overflowMode = TMPro.TextOverflowModes.Overflow;
            placeholderText.enableAutoSizing = true;
            placeholderText.fontSizeMin = 10;
            placeholderText.fontSizeMax = GlobalElementSettings.MaxFontSize;
            inputField.placeholder = placeholderText;
            
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(inputFieldObj.transform, false);
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Input Field Text
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = "";
            tmpText.fontSize = GlobalElementSettings.MaxFontSize;
            tmpText.color = GlobalElementSettings.ElementTextColor;
            tmpText.alignment = TMPro.TextAlignmentOptions.Center;
            tmpText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            tmpText.overflowMode = TMPro.TextOverflowModes.Overflow;
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 10;
            tmpText.fontSizeMax = GlobalElementSettings.MaxFontSize;
            
            inputField.textComponent = tmpText;
            inputField.onValueChanged.AddListener((input) => settings.OnValueChanged?.Invoke(inputField));
            
            var image = inputFieldObj.GetComponent<Image>();
            image.color = GlobalElementSettings.ElementBackgroundColor;

            m_panelElements[panelObj].Add(inputField);
            return true;
        }

        private bool AddButton(GameObject panelObj, ButtonSettings settings)
        {
            if (panelObj == null)
                return false;

            GameObject buttonObj = new GameObject(settings.Name, typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            
            buttonObj.transform.SetParent(panelObj.transform, false);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            if (buttonRect == null)
                return false;

            UpdateElementSizes(panelObj, buttonRect, m_panelElements[panelObj].Count);

            UIButton button = buttonObj.AddComponent<UIButton>();
            if (button == null)
                return false;

            // Button Text
            GameObject textObj = new GameObject("Label", typeof(RectTransform));
            textObj.transform.SetParent(buttonObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var image = buttonObj.GetComponent<Image>();
            image.color = GlobalElementSettings.ElementBackgroundColor;

            // Button Label
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = settings.Label;
            tmpText.alignment = TMPro.TextAlignmentOptions.Center;
            tmpText.fontSize = GlobalElementSettings.MaxFontSize;
            tmpText.color = GlobalElementSettings.ElementTextColor;
            tmpText.textWrappingMode = TMPro.TextWrappingModes.Normal;
            tmpText.overflowMode = TMPro.TextOverflowModes.Overflow;
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = 10; 
            tmpText.fontSizeMax = GlobalElementSettings.MaxFontSize;

            button.OnClicked.AddListener((btn => settings.OnClick?.Invoke(btn)));
            button.OnMouseOver.AddListener((btn) => btn.CanvasGroup.alpha = 0.8f);
            button.OnMouseOut.AddListener((btn) => btn.CanvasGroup.alpha = 1f);

            m_panelElements[panelObj].Add(button);
            return true;
        }

        public void Update()
        {
            if (!IsValid())
                return;

            foreach (var panel in m_panelElements)
            {
                if (m_lastPanelSize == panel.Key.GetComponent<RectTransform>().rect.size)
                    continue;
                
                for (var index = 0; index < panel.Value.Count; index++)
                    UpdateElementSizes(panel.Key, panel.Value[index].GetComponent<RectTransform>(), index);

                // Make sure the panels are in front of other UI
                m_lastPanelSize = panel.Key.GetComponent<RectTransform>().rect.size;
            }
            
            // Log the current random state of the game to the log panel
            foreach (var panel in m_panelElements)
            {
                if (panel.Key.name != "Log Panel")
                    continue;

                var logText = panel.Value.OfType<TextMeshProUGUI>().FirstOrDefault(t => t.gameObject.name == "LogInfoText");
                var keyLogText = panel.Value.OfType<TextMeshProUGUI>().FirstOrDefault(t => t.gameObject.name == "LogKeys");
                if (logText == null || keyLogText == null)
                    continue;
                
                // Copy the state to avoid modifying the actual game state
                UnityEngine.Random.State originalState = UnityEngine.Random.state;
                logText.text = Random.value.ToString(CultureInfo.CurrentCulture);
                UnityEngine.Random.state = originalState;
                
                TasController tasController = TasServices.TasController as TasController;
                if (tasController?.m_inputThisFrame == null) continue;
                keyLogText.text = (tasController?.m_inputThisFrame).Distinct().Aggregate("", (current, key) => current + (key + " "));
            }
        }

        private void StartTas(UIButton button)
        {
            if (!IsValid())
                return;
            
            // Get the save number from the input field
            var inputField = m_panelElements[button.transform.parent.gameObject]
                .OfType<TMP_InputField>()
                .FirstOrDefault(f => f.gameObject.name == "SaveNumberInput");
            if (inputField == null)
            {
                TasServices.Log.Log("Could not find SaveNumberInput field.");
                return;
            }
            
            if (!int.TryParse(inputField.text, out int saveNumber))
            {
                TasServices.Log.Log("Invalid save number entered.");
                return;
            }

            TasServices.Log.Log("Starting TAS...");
            TasServices.TasController.StartTas(saveNumber - 1);
        }

        public bool Initialize()
        {
            var settings = new PanelSettings
            {
                Name = "Main Panel",
                StartingPosition = new Vector2(50, -50),
                StartingSize = new Vector2(600, 400),
                BackgroundColor = new Color(0f, 0f, 0f, 0.85f),
                MinSize = new Vector2(200, 100),
                TitleBarColor = new Color(0.1f, 0.1f, 0.1f, 1f),
                TitleTextColor = Color.white,
                TitleHeight = 30
            };
            
            if (!CreateCanvas(settings, out var mainPanel))
            {
                TasServices.Log.Log($"[{Name}]: Failed to create TAS Main Canvas");
                return false;
            }

            var logSettings = settings;
            logSettings.Name = "Log Panel";
            logSettings.StartingPosition = new Vector2(Screen.width - settings.StartingSize.x - 50, -50);
            if (!CreateCanvas(logSettings, out var logPanel))
            {
                TasServices.Log.Log($"[{Name}]: Failed to create TAS Log Canvas");
                return false;
            }
            
            var tasSettings = settings;
            tasSettings.Name = "Tas Control Panel";
            tasSettings.StartingPosition = new Vector2(50, -settings.StartingSize.y - settings.StartingPosition.y - 70);
            if (!CreateCanvas(tasSettings, out var tasPanel))
            {
                TasServices.Log.Log($"[{Name}]: Failed to create Tas Canvas");
                return false;
            }
            
            AddInputField(mainPanel, new InputFieldSettings
            {
                Name = "SaveNumberInput",
                Placeholder = "Enter Save Number...",
                OnValueChanged = null
            });
            
            AddButton(mainPanel, new ButtonSettings
            {
                Name = "StartButton",
                Label = "Start TAS",
                OnClick = StartTas
            });
            
            AddButton(mainPanel, new ButtonSettings
            {
                Name = "StopTas",
                Label = "Stop TAS",
                OnClick = (btn => TasServices.TasController.StopTas())
            });
            
            AddButton(tasPanel, new ButtonSettings
            {
                Name = "PlayAtNormalSpeedButton",
                Label = "Play at Normal Speed",
                OnClick = (btn => TasServices.TasController.PlayAtNormalSpeed())
            });
            
            AddButton(tasPanel, new ButtonSettings
            {
                Name = "PlayAtFrameSpeedButton",
                Label = "Play at Frame Speed",
                OnClick = (btn => TasServices.TasController.PlayAtFrameSpeed())
            });
            
            AddDropdown(tasPanel, new DropdownSettings
            {
                Name = "ChoosePlaybackFile",
                MaxVisibleOptions = 5,
                Options = TasServices.File
                    .GetFilesInDirectory(Path.Combine(TasServices.File.PathToTasDir, "Runs"), "*.tas", true)
                    .Select(f => TasServices.File.GetDirectoryName(f, out var dirName) ? dirName.Split(Path.DirectorySeparatorChar).Last() : "")
                    .Where(d => !string.IsNullOrEmpty(d)).Reverse().ToList(),
                DefaultIndex = 0,
            });

            AddButton(tasPanel, new ButtonSettings
            {
                Name = "Playback",
                Label = "Playback",
                OnClick = (btn =>
                {
                    var dropdown = m_panelElements[btn.transform.parent.gameObject]
                        .OfType<TMP_Dropdown>()
                        .FirstOrDefault(d => d.gameObject.name == "ChoosePlaybackFile");
                    if (dropdown == null)
                    {
                        TasServices.Log.Log("Could not find ChoosePlaybackFile dropdown.");
                        return;
                    }

                    var selectedFile = dropdown.options[dropdown.value].text;
                    var playbackFilePath = Path.Combine(TasServices.File.PathToTasDir, "Runs", selectedFile);
                    TasServices.TasController.Playback(playbackFilePath);
                })
            });
            
            AddTextElement(logPanel, new TextElementSettings
            {
                Name = "LogInfoText",
                Text = "TAS Initialized. Enter a save number and click Start TAS.",
                FontSize = GlobalElementSettings.MaxFontSize,
                TextColor = Color.white
            });
            
            AddTextElement(logPanel, new TextElementSettings
            {
                Name = "LogKeys",
                Text = "",
                FontSize = GlobalElementSettings.MaxFontSize,
                TextColor = Color.white
            });

            m_isInitialized = true;
            return true;
        }

        public bool IsValid()
        {
            return m_isInitialized && m_panelElements.Count > 0 && m_panelElements.All(p => p.Key != null);
        }

        public void Hide()
        {
            if (!IsValid())
                return;
            
            m_panelElements.Keys.ToList().ForEach(panel => panel.SetActive(false));
            
            m_isShowing = false;
        }
        
        public bool IsShowing()
        {
            if (!IsValid())
                return false;

            return m_isShowing;
        }
        
        public void Show()
        {
            if (!IsValid())
                return;
            
            m_panelElements.Keys.ToList().ForEach(panel => panel.SetActive(true));
            
            m_isShowing = true;
        }

        // IService 
        
        public void CollectDebugState(Dictionary<string, object> debugStateProperties)
        {
        }

        public void Shutdown()
        {
        }
        
        public IEnumerator InitializeAsync()
        {
            yield return null;
        }
        
        public struct PanelSettings
        {
            public string Name;
            public Vector2 StartingPosition;
            public Vector2 StartingSize;
            public Color BackgroundColor;
            
            public Color TitleBarColor;
            public Color TitleTextColor;
            public int TitleHeight;
            
            public Vector2 MinSize;
        }
        
        public struct GlobalLayoutSettings
        {
            public Vector2 Padding;
            public Vector2 ElementSize;
            public Color ElementBackgroundColor;
            public Color ElementTextColor;
            public int MaxElementsPerRow;
            public int MaxFontSize;
        }
        private struct TextElementSettings
        {
            public string Name;
            public string Text;
            public int FontSize;
            public Color TextColor;
        }
        private struct DropdownSettings
        {
            public string Name;
            public int DefaultIndex;
            public int MaxVisibleOptions; // Make sure it fits on screen lol
            public List<string> Options;
            public UnityAction<TMP_Dropdown> OnValueChanged;
        }
        
        private struct InputFieldSettings
        {
            public string Name;
            public string Placeholder;
            public UnityAction<TMP_InputField> OnValueChanged;
        }
        
        private struct ButtonSettings
        {
            public string Name;
            public string Label;
            public UnityAction<UIButton> OnClick;
        }
        
        private class DraggableResizeHandle : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
        {
            private RectTransform m_panelRect;
            private RectTransform m_parentRect;
            private Vector2 m_startMousePos;
            private Vector2 m_startSize;
            private PanelSettings _mPanelSizeSettings;

            public void Init(RectTransform panelRect, PanelSettings panelSettings)
            {
                m_panelRect = panelRect;
                m_parentRect = m_panelRect.parent as RectTransform;
                _mPanelSizeSettings = panelSettings;
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_panelRect, eventData.position, eventData.pressEventCamera, out m_startMousePos);
                m_startSize = m_panelRect.sizeDelta;
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (eventData.position.x > Screen.width || eventData.position.y > Screen.height ||
                    eventData.position.x < 0 || eventData.position.y < 0)
                    return;
                
                var maxHeight = m_parentRect.rect.height - m_panelRect.anchoredPosition.y - _mPanelSizeSettings.TitleHeight;
                var maxWidth = m_parentRect.rect.width - m_panelRect.anchoredPosition.x;
                Vector2 currentMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    m_panelRect, eventData.position, eventData.pressEventCamera, out currentMousePos);
                Vector2 delta = currentMousePos - m_startMousePos;
                Vector2 newSize = m_startSize + new Vector2(delta.x, -delta.y);
                newSize.x = Mathf.Clamp(newSize.x, _mPanelSizeSettings.MinSize.x, maxWidth);
                newSize.y = Mathf.Clamp(newSize.y, _mPanelSizeSettings.MinSize.y, maxHeight);
                m_panelRect.sizeDelta = newSize;
            }

            public void OnEndDrag(PointerEventData eventData) { }
        }
        private class DraggablePanel : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            private RectTransform m_rectTransform;
            private RectTransform m_parentRect;
            private PanelSettings m_panelSettings;
            private Vector2 m_offset;

            public void Init(PanelSettings panelSettings)
            {
                m_panelSettings = panelSettings;
            }
            
            private void Awake()
            {
                m_rectTransform = GetComponent<RectTransform>();
                m_parentRect = m_rectTransform.parent as RectTransform;
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                transform.SetAsLastSibling();
                
                Vector2 mouseLocalPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        m_parentRect, eventData.position, eventData.pressEventCamera, out mouseLocalPoint))
                {
                    m_offset = m_rectTransform.anchoredPosition - mouseLocalPoint;
                }
            }

            public void OnBeginDrag(PointerEventData eventData) { }

            public void OnDrag(PointerEventData eventData)
            {
                Vector2 mouseLocalPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        m_parentRect, eventData.position, eventData.pressEventCamera, out mouseLocalPoint))
                {
                    var newPos = mouseLocalPoint + m_offset;
                    Vector2 clampedPos = new Vector2(
                        Mathf.Clamp(newPos.x, 0, m_parentRect.rect.width - m_rectTransform.rect.width),
                        Mathf.Clamp(newPos.y, -m_parentRect.rect.height + m_rectTransform.rect.height, -m_panelSettings.TitleHeight)
                    );
                    m_rectTransform.anchoredPosition = clampedPos;
                }
            }

            public void OnEndDrag(PointerEventData eventData) { }
        }
    }
}