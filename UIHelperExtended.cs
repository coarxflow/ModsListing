using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEngine;

namespace ModsList.KlyteExtensions
{
	public class UIHelperExtension : UIHelperBase
	{
		public static readonly string kButtonTemplate = "OptionsButtonTemplate";
		
		public static readonly string kGroupTemplate = "OptionsGroupTemplate";
		
		public static readonly string kDropdownTemplate = "OptionsDropdownTemplate";
		
		public static readonly string kCheckBoxTemplate = "OptionsCheckBoxTemplate";
		
		public static readonly string kSliderTemplate = "OptionsSliderTemplate";
		
		public static readonly string kTextfieldTemplate = "OptionsTextfieldTemplate";
		
		private UIComponent m_Root;
		
		public static string version
		{
			get
			{
				return string.Concat(new object[]
				                     {
					typeof(UIHelperExtension).Assembly.GetName().Version.Major,
					".",
					typeof(UIHelperExtension).Assembly.GetName().Version.Minor,
					".",
					typeof(UIHelperExtension).Assembly.GetName().Version.Build
				});
			}
		}
		
		public UIComponent self
		{
			get
			{
				return this.m_Root;
			}
		}
		
		public object AddButton(string text, OnButtonClicked eventCallback)
		{
			bool flag = eventCallback != null && !string.IsNullOrEmpty(text);
			object result;
			if (flag)
			{
                UIButton uIButton = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kButtonTemplate)) as UIButton;
				uIButton.text = text;
				uIButton.eventClick += delegate(UIComponent c, UIMouseEventParameter sel)
				{
					eventCallback();
				};
				result = uIButton;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create button with no name or no event");
				result = null;
			}
			return result;
		}
		
		public object AddCheckbox(string text, bool defaultValue, OnCheckChanged eventCallback)
		{
			bool flag = eventCallback != null && !string.IsNullOrEmpty(text);
			object result;
			if (flag)
			{
				UICheckBox uICheckBox = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kCheckBoxTemplate)) as UICheckBox;
				uICheckBox.text = text;
				uICheckBox.isChecked = defaultValue;
				uICheckBox.eventCheckChanged += delegate(UIComponent c, bool isChecked)
				{
					eventCallback(isChecked);
				};
				result = uICheckBox;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create checkbox with no name or no event");
				result = null;
			}
			return result;
		}
		
		public object AddDropdown(string text, string[] options, int defaultSelection, OnDropdownSelectionChanged eventCallback)
		{
			UIDropDown uIDropDown = this.AddDropdownBase(text, options, eventCallback);
			bool flag = uIDropDown != null;
			object result;
			if (flag)
			{
				uIDropDown.selectedIndex = defaultSelection;
				result = uIDropDown;
			}
			else
			{
				result = null;
			}
			return result;
		}
		
		public UIDropDown AddDropdown(string text, string[] options, string defaultSelection, OnDropdownSelectionChanged eventCallback)
		{
			UIDropDown uIDropDown = this.AddDropdownBase(text, options, eventCallback);
			bool flag = uIDropDown != null;
			UIDropDown result;
			if (flag)
			{
				bool flag2 = options.Contains(defaultSelection);
				bool flag3 = flag2;
				if (flag3)
				{
					uIDropDown.selectedIndex = options.ToList<string>().IndexOf(defaultSelection);
				}
				result = uIDropDown;
			}
			else
			{
				result = null;
			}
			return result;
		}
		
		private UIDropDown AddDropdownBase(string text, string[] options, OnDropdownSelectionChanged eventCallback)
		{
			return UIHelperExtension.CloneBasicDropDown(text, options, eventCallback, this.m_Root);
		}
		
		public static UIDropDown CloneBasicDropDown(string text, string[] options, OnDropdownSelectionChanged eventCallback, UIComponent parent)
		{
			bool flag = eventCallback != null && !string.IsNullOrEmpty(text);
			UIDropDown result;
			if (flag)
			{
				UIPanel uIPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
				uIPanel.Find<UILabel>("Label").text = text;
				UIDropDown uIDropDown = uIPanel.Find<UIDropDown>("Dropdown");
				uIDropDown.items = options;
				uIDropDown.eventSelectedIndexChanged += delegate(UIComponent c, int sel)
				{
					eventCallback(sel);
				};
				result = uIDropDown;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create dropdown with no name or no event");
				result = null;
			}
			return result;
		}
		
		public object AddSlider(string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback)
		{
			bool flag = eventCallback != null && !string.IsNullOrEmpty(text);
			object result;
			if (flag)
			{
				UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kSliderTemplate)) as UIPanel;
				uIPanel.Find<UILabel>("Label").text = text;
				UISlider uISlider = uIPanel.Find<UISlider>("Slider");
				uISlider.minValue = min;
				uISlider.maxValue = max;
				uISlider.stepSize = step;
				uISlider.value = defaultValue;
				uISlider.eventValueChanged += delegate(UIComponent c, float val)
				{
					eventCallback(val);
				};
				result = uISlider;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create slider with no name or no event");
				result = null;
			}
			return result;
		}
		
		public object AddSpace(int height)
		{
			bool flag = height > 0;
			object result;
			if (flag)
			{
				UIPanel uIPanel = this.m_Root.AddUIComponent<UIPanel>();
				uIPanel.name = "Space";
				uIPanel.isInteractive = false;
				uIPanel.height = (float)height;
				result = uIPanel;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create space of " + height + " height");
				result = null;
			}
			return result;
		}
		
		public UIHelperExtension(UIComponent panel)
		{
			this.m_Root = panel;
		}
		
		public UIHelperExtension(UIHelper panel)
		{
			this.m_Root = (UIComponent)panel.self;
		}
		
		public UIHelperExtension AddGroupExtended(string text)
		{
			return (UIHelperExtension)this.AddGroup(text);
		}
		
		public UIHelperBase AddGroup(string text)
		{
			bool flag = !string.IsNullOrEmpty(text);
			UIHelperBase result;
			if (flag)
			{
				UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kGroupTemplate)) as UIPanel;
				uIPanel.Find<UILabel>("Label").text = text;
				result = new UIHelperExtension(uIPanel.Find("Content"));
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create group with no name");
				result = null;
			}
			return result;
		}
		
		public object AddTextfield(string text, string defaultContent, OnTextChanged eventChangedCallback, OnTextSubmitted eventSubmittedCallback)
		{
			bool flag = eventChangedCallback != null && !string.IsNullOrEmpty(text);
			object result;
			if (flag)
			{
				UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kTextfieldTemplate)) as UIPanel;
				uIPanel.Find<UILabel>("Label").text = text;
				UITextField uITextField = uIPanel.Find<UITextField>("Text Field");
				uITextField.text = defaultContent;
				uITextField.eventTextChanged += delegate(UIComponent c, string sel)
				{
					eventChangedCallback(sel);
				};
				uITextField.eventTextSubmitted += delegate(UIComponent c, string sel)
				{
					bool flag2 = eventSubmittedCallback != null;
					if (flag2)
					{
						eventSubmittedCallback(sel);
					}
				};
				result = uITextField;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create dropdown with no name or no event");
				result = null;
			}
			return result;
		}
		
		public UITextField AddTextField(string name, OnTextChanged eventCallback, string defaultValue = "", OnTextSubmitted eventSubmit = null)
		{
			return (UITextField)this.AddTextfield(name, defaultValue, eventCallback, eventSubmit);
		}
		
		public UITextField AddPasswordField(string name, OnTextChanged eventCallback)
		{
			bool flag = eventCallback != null && !string.IsNullOrEmpty(name);
			UITextField result;
			if (flag)
			{
				UITextField uITextField = (UITextField)this.AddTextfield(name, "", eventCallback, null);
				uITextField.isPasswordField = true;
				result = uITextField;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create textField with no name or no event");
				result = null;
			}
			return result;
		}
		
		public UILabel AddLabel(string name)
		{
			UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
			uIPanel.autoFitChildrenVertically = true;
			UILabel uILabel = uIPanel.Find<UILabel>("Label");
			uILabel.text = name;
			uILabel.maximumSize = new Vector2(700f, 9999f);
			uILabel.minimumSize = new Vector2(700f, 0f);
			uILabel.wordWrap = true;
			UnityEngine.Object.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);
			return uILabel;
		}
		
		public UITextureSprite AddNamedTexture(string name)
		{
			UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
			uIPanel.Find<UILabel>("Label").text = name;
			UnityEngine.Object.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);
			UITextureSprite uITextureSprite = uIPanel.AddUIComponent<UITextureSprite>();
			uITextureSprite.isVisible = true;
			uITextureSprite.name = "TextureSprite";
			return uITextureSprite;
		}
		
		public DropDownColorSelector AddColorField(string name, Color defaultValue, OnColorChanged eventCallback, OnButtonClicked eventRemove)
		{
			bool flag = eventCallback != null && !string.IsNullOrEmpty(name);
			DropDownColorSelector result;
			if (flag)
			{
				UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
				uIPanel.name = "DropDownColorSelector";
				uIPanel.Find<UILabel>("Label").text = name;
				UnityEngine.Object.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);
				DropDownColorSelector dropDownColorSelector = new DropDownColorSelector(uIPanel, defaultValue, 0);
				dropDownColorSelector.eventColorChanged += delegate(Color32 value)
				{
					eventCallback(value);
				};
				dropDownColorSelector.eventOnRemove += delegate
				{
					bool flag2 = eventRemove != null;
					if (flag2)
					{
						eventRemove();
					}
				};
				result = dropDownColorSelector;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create colorPicker with no name or no event");
				result = null;
			}
			return result;
		}
		
		public NumberedColorList AddNumberedColorList(string name, List<Color32> defaultValues, OnButtonSelect<int> eventCallback, UIComponent addButtonContainer, OnButtonClicked eventAdd)
		{
			bool flag = eventCallback != null;
			NumberedColorList result;
			if (flag)
			{
				UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
				uIPanel.name = "NumberedColorList";
				bool flag2 = string.IsNullOrEmpty(name);
				if (flag2)
				{
					uIPanel.Find<UILabel>("Label").text = "";
				}
				else
				{
					uIPanel.Find<UILabel>("Label").text = name;
				}
				UnityEngine.Object.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);
				NumberedColorList numberedColorList = new NumberedColorList(uIPanel, defaultValues, addButtonContainer);
				numberedColorList.eventOnClick += delegate(int value)
				{
					eventCallback(value);
				};
				numberedColorList.eventOnAdd += delegate
				{
					bool flag3 = eventAdd != null;
					if (flag3)
					{
						eventAdd();
					}
				};
				result = numberedColorList;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create colorPicker with no name or no event");
				result = null;
			}
			return result;
		}
		
		public TextList<T> AddTextList<T>(string name, Dictionary<T, string> defaultValues, OnButtonSelect<T> eventCallback, int width, int height)
		{
			bool flag = eventCallback != null;
			TextList<T> result;
			if (flag)
			{
				UIPanel uIPanel = this.m_Root.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
				uIPanel.name = "NumberedColorList";
				bool flag2 = string.IsNullOrEmpty(name);
				if (flag2)
				{
					uIPanel.Find<UILabel>("Label").text = "";
				}
				else
				{
					uIPanel.Find<UILabel>("Label").text = name;
				}
				UnityEngine.Object.Destroy(uIPanel.Find<UIDropDown>("Dropdown").gameObject);
				TextList<T> textList = new TextList<T>(uIPanel, defaultValues, width, height, name);
				textList.eventOnClick += delegate(T value)
				{
					eventCallback(value);
				};
				result = textList;
			}
			else
			{
				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, "Cannot create colorPicker with no name or no event");
				result = null;
			}
			return result;
		}
	}

	internal class UIButtonWithId : UIButton
	{
		public object id;
	}

	public class TextList<T>
	{
		public delegate void addItemCallback(int idx, string text);
		
		private UIScrollablePanel linesListPanel;
		
		private UIComponent m_parent;
		
		private Dictionary<T, string> m_itemsList;
		
		private T _selectedItem;
		
		public string name;
		
		public event OnButtonSelect<T> eventOnClick;
		
		public event OnButtonClicked eventOnAdd;
		
		public T selectedItem
		{
			get
			{
				return this._selectedItem;
			}
			internal set
			{
				this._selectedItem = value;
				this.refreshSelection();
			}
		}
		
		public Dictionary<T, string> itemsList
		{
			get
			{
				return this.m_itemsList;
			}
			set
			{
				bool flag = value == null;
				if (flag)
				{
					value = new Dictionary<T, string>();
				}
				this.m_itemsList = value;
				this.selectedItem = default(T);
				this.redrawButtons();
			}
		}
		
		public UIPanel root
		{
			get
			{
				return this.linesListPanel.transform.GetComponentInParent<UIPanel>();
			}
		}
		
		public bool unselected
		{
			get
			{
				bool arg_33_0;
				if (this.selectedItem != null)
				{
					T selectedItem = this.selectedItem;
					arg_33_0 = selectedItem.Equals(default(T));
				}
				else
				{
					arg_33_0 = true;
				}
				return arg_33_0;
			}
		}
		
		public void Enable()
		{
			this.linesListPanel.enabled = true;
			this.redrawButtons();
		}
		
		public void Disable()
		{
			foreach (Transform transform in this.linesListPanel.transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			this.linesListPanel.enabled = false;
		}
		
		public KeyValuePair<T, string> popSelected()
		{
			KeyValuePair<T, string> result = default(KeyValuePair<T, string>);
			bool flag = this.m_itemsList.ContainsKey(this.selectedItem);
			if (flag)
			{
				result = this.m_itemsList.First(delegate(KeyValuePair<T, string> x)
				                                {
					T key = x.Key;
					return key.Equals(this.selectedItem);
				});
				this.m_itemsList.Remove(this.selectedItem);
			}
			this.selectedItem = default(T);
			this.redrawButtons();
			return result;
		}
		
		public void addItemToList(T id, string name)
		{
			this.m_itemsList[id] = name;
			this.redrawButtons();
		}
		
		public TextList(UIComponent parent, Dictionary<T, string> initiaItemList, int width, int height, string name)
		{
			this.name = name;
			this.m_parent = parent;
			((UIPanel)parent).autoFitChildrenVertically = true;
			((UIPanel)parent).padding = new RectOffset(20, 20, 20, 20);
			UIPanel uIPanel = this.m_parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel;
			uIPanel.name = "TextList";
			uIPanel.height = (float)height;
			uIPanel.width = (float)width;
			uIPanel.autoLayoutDirection = LayoutDirection.Vertical;
			uIPanel.autoLayoutStart = LayoutStart.TopLeft;
			uIPanel.autoFitChildrenVertically = true;
			uIPanel.wrapLayout = true;
			uIPanel.padding = new RectOffset(0, 0, 0, 0);
			uIPanel.clipChildren = true;
			uIPanel.pivot = UIPivotPoint.MiddleCenter;
			uIPanel.relativePosition = Vector2.zero;
			foreach (Transform transform in uIPanel.transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			GameObject gameObject = new GameObject("Lines Listing Scroll", new Type[]
			                                       {
				typeof(UIScrollablePanel)
			});
			this.linesListPanel = gameObject.GetComponent<UIScrollablePanel>();
			this.linesListPanel.autoLayout = false;
			this.linesListPanel.width = (float)width;
			this.linesListPanel.height = (float)height;
			this.linesListPanel.useTouchMouseScroll = true;
			this.linesListPanel.scrollWheelAmount = 20;
			this.linesListPanel.eventMouseWheel += delegate(UIComponent component, UIMouseEventParameter eventParam)
			{
				this.linesListPanel.scrollPosition -= new Vector2(0f, eventParam.wheelDelta * (float)this.linesListPanel.scrollWheelAmount);
			};
			uIPanel.AttachUIComponent(this.linesListPanel.gameObject);
			this.linesListPanel.autoLayout = true;
			this.linesListPanel.autoLayoutDirection = LayoutDirection.Vertical;
			this.linesListPanel.useTouchMouseScroll = true;
			this.linesListPanel.scrollWheelAmount = 20;
			this.linesListPanel.eventMouseWheel += delegate(UIComponent component, UIMouseEventParameter eventParam)
			{
				this.linesListPanel.scrollPosition -= new Vector2(0f, eventParam.wheelDelta * (float)this.linesListPanel.scrollWheelAmount);
				eventParam.Use();
			};
			foreach (Transform transform2 in this.linesListPanel.transform)
			{
				UnityEngine.Object.Destroy(transform2.gameObject);
			}
			this.itemsList = initiaItemList;
		}
		
		private static void initButton(UIButton button, string baseSprite)
		{
			button.normalBgSprite = baseSprite;
			button.disabledBgSprite = baseSprite;
			button.hoveredBgSprite = baseSprite;
			button.focusedBgSprite = baseSprite;
			button.pressedBgSprite = baseSprite;
			button.textColor = new Color32(255, 255, 255, 255);
			button.pressedTextColor = Color.red;
			button.hoveredTextColor = Color.gray;
		}
		
		public void Redraw()
		{
			this.redrawButtons();
		}
		
		private void redrawButtons()
		{
			foreach (Transform transform in this.linesListPanel.transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			foreach (KeyValuePair<T, string> current in this.m_itemsList)
			{
				UIButtonWithId itemButton = new GameObject
				{
					transform = 
					{
						parent = this.linesListPanel.transform
					}
				}.AddComponent<UIButtonWithId>();
				itemButton.width = this.linesListPanel.width;
				itemButton.height = 35f;
				TextList<T>.initButton(itemButton, "EmptySprite");
				itemButton.hoveredColor = Color.gray;
				itemButton.pressedColor = Color.black;
				itemButton.focusedColor = Color.black;
				itemButton.color = new Color(0f, 0f, 0f, 0.7f);
				itemButton.textColor = Color.white;
				itemButton.focusedTextColor = Color.white;
				itemButton.hoveredTextColor = Color.white;
				itemButton.pressedTextColor = Color.white;
				itemButton.outlineColor = Color.black;
				itemButton.useOutline = true;
				itemButton.id = current.Key;
				itemButton.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
				{
					this.selectedItem = (T)((object)itemButton.id);
					eventParam.Use();
				};
				itemButton.text = current.Value;
				itemButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
				itemButton.name = string.Format("[{1}] {0}", current.Value, current.Key);
			}
		}
		
		private void refreshSelection()
		{
			foreach (Transform transform in this.linesListPanel.transform)
			{
				UIButtonWithId component = transform.GetComponent<UIButtonWithId>();
				bool flag = component.id.Equals(this.selectedItem);
				if (flag)
				{
					component.color = new Color(255f, 255f, 255f, 1f);
					component.textColor = Color.black;
					component.focusedTextColor = Color.black;
					component.hoveredTextColor = Color.black;
					component.pressedTextColor = Color.black;
					component.hoveredColor = Color.white;
					component.pressedColor = Color.white;
					component.focusedColor = Color.white;
				}
				else
				{
					component.color = new Color(0f, 0f, 0f, 0.7f);
					component.textColor = Color.white;
					component.focusedTextColor = Color.white;
					component.hoveredTextColor = Color.white;
					component.pressedTextColor = Color.white;
				}
			}
		}
		
		public void unselect()
		{
			this.selectedItem = default(T);
		}
	}

	public delegate void OnMultipleColorChanged(List<Color32> val);
	public delegate void OnColorChanged(Color val);
	public delegate void OnButtonSelect<T>(T idx);

	public class NumberedColorList
	{
		private class UIButtonWithId : UIButton
		{
			public int id;
		}
		
		private UIPanel linesListPanel;
		
		private UIComponent m_parent;
		
		private List<Color32> m_colorList;
		
		private UIButton m_add;
		
		public event OnButtonSelect<int> eventOnClick;
		
		public event OnButtonClicked eventOnAdd;
		
		public List<Color32> colorList
		{
			get
			{
				return this.m_colorList;
			}
			set
			{
				this.m_colorList = value;
				this.redrawButtons();
			}
		}
		
		public void Enable()
		{
			this.linesListPanel.enabled = true;
			this.m_add.enabled = true;
			this.redrawButtons();
		}
		
		public void Disable()
		{
			foreach (Transform transform in this.linesListPanel.transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			this.m_add.enabled = false;
			this.linesListPanel.enabled = false;
		}
		
		public NumberedColorList(UIComponent parent, List<Color32> initialColorList, UIComponent addButtonContainer)
		{
			this.m_parent = parent;
			parent.width = 500f;
			((UIPanel)parent).autoFitChildrenVertically = true;
			this.linesListPanel = (this.m_parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel);
			this.linesListPanel.name = "NumberedColorList";
			this.linesListPanel.height = 40f;
			this.linesListPanel.width = 500f;
			this.linesListPanel.autoLayoutDirection = LayoutDirection.Horizontal;
			this.linesListPanel.autoLayoutStart = LayoutStart.TopLeft;
			this.linesListPanel.autoFitChildrenVertically = true;
			this.linesListPanel.wrapLayout = true;
			this.linesListPanel.autoLayoutPadding = new RectOffset(5, 5, 5, 5);
			foreach (Transform transform in this.linesListPanel.transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			this.m_add = (addButtonContainer.GetComponentInChildren<UILabel>().AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kButtonTemplate)) as UIButton);
			this.m_add.text = "+";
			this.m_add.autoSize = false;
			this.m_add.height = 27f;
			this.m_add.width = 27f;
			this.m_add.relativePosition = new Vector3(70f, 0f, 0f);
			this.m_add.textPadding = new RectOffset(0, 0, 0, 0);
			this.m_add.textHorizontalAlignment = UIHorizontalAlignment.Center;
			this.m_add.eventClick += delegate(UIComponent c, UIMouseEventParameter sel)
			{
				this.m_colorList.Add(Color.white);
				this.redrawButtons();
				bool flag = this.eventOnAdd != null;
				if (flag)
				{
					this.eventOnAdd();
				}
			};
			this.colorList = initialColorList;
		}
		
		private static void initButton(UIButton button, string baseSprite)
		{
			button.normalBgSprite = baseSprite;
			button.disabledBgSprite = baseSprite;
			button.hoveredBgSprite = baseSprite;
			button.focusedBgSprite = baseSprite;
			button.pressedBgSprite = baseSprite;
			button.textColor = new Color32(255, 255, 255, 255);
			button.pressedTextColor = Color.red;
			button.hoveredTextColor = Color.gray;
		}
		
		public void Redraw()
		{
			this.redrawButtons();
		}
		
		private void redrawButtons()
		{
			foreach (Transform transform in this.linesListPanel.transform)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
			for (int i = 0; i < this.colorList.Count; i++)
			{
				NumberedColorList.UIButtonWithId itemButton = new GameObject
				{
					transform = 
					{
						parent = this.linesListPanel.transform
					}
				}.AddComponent<NumberedColorList.UIButtonWithId>();
				itemButton.width = 35f;
				itemButton.height = 35f;
				NumberedColorList.initButton(itemButton, "EmptySprite");
				itemButton.color = this.colorList[i];
				itemButton.hoveredColor = itemButton.color;
				itemButton.pressedColor = itemButton.color;
				itemButton.focusedColor = itemButton.color;
				itemButton.textColor = Color.white;
				itemButton.hoveredColor = itemButton.textColor;
				itemButton.id = i + 1;
				itemButton.eventClick += delegate(UIComponent component, UIMouseEventParameter eventParam)
				{
					bool flag = this.eventOnClick != null;
					if (flag)
					{
						this.eventOnClick(itemButton.id);
					}
				};
				this.setLineNumberMainListing(i + 1, itemButton);
				itemButton.name = "Color #" + (i + 1);
			}
		}
		
		private void setLineNumberMainListing(int num, UIButton button)
		{
			UILabel uILabel = button.AddUIComponent<UILabel>();
			uILabel.autoSize = false;
			uILabel.autoHeight = false;
			uILabel.pivot = UIPivotPoint.TopLeft;
			uILabel.verticalAlignment = UIVerticalAlignment.Middle;
			uILabel.textAlignment = UIHorizontalAlignment.Center;
			uILabel.relativePosition = new Vector3(0f, 0f);
			uILabel.width = button.width;
			uILabel.height = button.height;
			uILabel.useOutline = true;
			uILabel.text = num.ToString();
			float num2 = uILabel.width / 50f;
			bool flag = uILabel.text.Length == 4;
			if (flag)
			{
				uILabel.textScale = num2;
				uILabel.relativePosition = new Vector3(0f, 1f);
			}
			else
			{
				bool flag2 = uILabel.text.Length == 3;
				if (flag2)
				{
					uILabel.textScale = num2 * 1.25f;
					uILabel.relativePosition = new Vector3(0f, 1.5f);
				}
				else
				{
					bool flag3 = uILabel.text.Length == 2;
					if (flag3)
					{
						uILabel.textScale = num2 * 1.75f;
						uILabel.relativePosition = new Vector3(-0.5f, 0.5f);
					}
					else
					{
						uILabel.textScale = num2 * 2.3f;
					}
				}
			}
		}
	}

	public class DropDownColorSelector
	{
		public delegate void ButtonClickHandler();
		
		public delegate void ColorChangeHandler(Color32 value);
		
		public delegate void ListColorChangeHandler(List<Color32> values);
		
		private UIDropDown m_r;
		
		private UIDropDown m_g;
		
		private UIDropDown m_b;
		
		private UILabel m_displayColor;
		
		private UILabel m_title;
		
		private UIComponent m_parent;
		
		private UIPanel uIPanel;
		
		private UIButton m_remove;
		
		private static string[] options;
		
		public int id;
		
		public event DropDownColorSelector.ColorChangeHandler eventColorChanged;
		
		public event DropDownColorSelector.ButtonClickHandler eventOnRemove;
		
		public Color32 selectedColor
		{
			get
			{
				return new Color32((byte)((this.m_r.selectedIndex == 0) ? 0 : (this.m_r.selectedIndex * 4 - 1)), (byte)((this.m_g.selectedIndex == 0) ? 0 : (this.m_g.selectedIndex * 4 - 1)), (byte)((this.m_b.selectedIndex == 0) ? 0 : (this.m_b.selectedIndex * 4 - 1)), 255);
			}
			set
			{
				this.setSelectedColor(value);
			}
		}
		
		public string title
		{
			get
			{
				bool flag = this.m_title != null;
				string result;
				if (flag)
				{
					result = this.m_title.text;
				}
				else
				{
					result = null;
				}
				return result;
			}
			set
			{
				bool flag = this.m_title != null;
				if (flag)
				{
					this.m_title.text = value;
				}
			}
		}
		
		public UIComponent parent
		{
			get
			{
				return this.m_parent;
			}
		}
		
		public void Disable()
		{
			this.uIPanel.enabled = false;
		}
		
		public void Enable()
		{
			this.uIPanel.enabled = true;
		}
		
		private void setSelectedColor(Color32 val)
		{
			this.m_r.selectedIndex = (int)((val.r + 1) / 4);
			this.m_g.selectedIndex = (int)((val.g + 1) / 4);
			this.m_b.selectedIndex = (int)((val.b + 1) / 4);
		}
		
		public DropDownColorSelector(UIComponent parent, Color initialColor, int id = 0)
		{
			this.id = 0;
			this.m_parent = parent;
			this.uIPanel = (this.m_parent.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kDropdownTemplate)) as UIPanel);
			this.uIPanel.name = "DropDownColorSelector";
			this.uIPanel.height = 40f;
			this.uIPanel.width = 280f;
			this.uIPanel.autoLayoutDirection = LayoutDirection.Horizontal;
			this.uIPanel.autoLayoutStart = LayoutStart.TopLeft;
			this.uIPanel.autoFitChildrenVertically = true;
			this.m_title = this.uIPanel.Find<UILabel>("Label");
			this.m_title.autoSize = false;
			this.m_title.height = 28f;
			this.m_title.width = 60f;
			this.m_title.textAlignment = UIHorizontalAlignment.Center;
			this.m_title.padding = new RectOffset(5, 5, 5, 5);
			this.m_r = this.uIPanel.Find<UIDropDown>("Dropdown");
			this.m_g = (this.uIPanel.AttachUIComponent(UnityEngine.Object.Instantiate<GameObject>(this.m_r.gameObject)) as UIDropDown);
			this.m_b = (this.uIPanel.AttachUIComponent(UnityEngine.Object.Instantiate<GameObject>(this.m_r.gameObject)) as UIDropDown);
			this.initializeDropDown(ref this.m_b);
			this.initializeDropDown(ref this.m_r);
			this.initializeDropDown(ref this.m_g);
			this.m_r.color = new Color32(255, 0, 0, 255);
			this.m_g.color = new Color32(0, 255, 0, 255);
			this.m_b.color = new Color32(0, 0, 255, 255);
			this.m_displayColor = (UILabel)this.uIPanel.AttachUIComponent(UnityEngine.Object.Instantiate<GameObject>(this.uIPanel.Find<UILabel>("Label").gameObject));
			this.m_displayColor.autoSize = false;
			this.m_displayColor.name = "Color result";
			this.m_displayColor.relativePosition += new Vector3(0f, 160f, 0f);
			this.m_displayColor.text = "";
			this.m_displayColor.height = 28f;
			this.m_displayColor.width = 100f;
			this.m_displayColor.textAlignment = UIHorizontalAlignment.Center;
			this.m_displayColor.backgroundSprite = "EmptySprite";
			this.m_displayColor.useOutline = true;
			this.m_displayColor.outlineColor = Color.black;
			this.m_displayColor.textColor = Color.white;
			this.m_displayColor.padding = new RectOffset(5, 5, 5, 5);
			this.m_remove = (this.uIPanel.AttachUIComponent(UITemplateManager.GetAsGameObject(UIHelperExtension.kButtonTemplate)) as UIButton);
			this.m_remove.text = "x";
			this.m_remove.autoSize = false;
			this.m_remove.height = 27f;
			this.m_remove.width = 27f;
			this.m_remove.textPadding = new RectOffset(0, 0, 0, 0);
			this.m_remove.textHorizontalAlignment = UIHorizontalAlignment.Center;
			this.m_remove.eventClick += delegate(UIComponent c, UIMouseEventParameter sel)
			{
				this.Disable();
				bool flag = this.eventOnRemove != null;
				if (flag)
				{
					this.eventOnRemove();
				}
			};
			this.setSelectedColor(initialColor);
		}
		
		private void initializeDropDown(ref UIDropDown dropDown)
		{
			bool flag = DropDownColorSelector.options == null;
			if (flag)
			{
				List<string> list = new List<string>();
				for (int i = 0; i <= 64; i++)
				{
					list.Add(string.Format("{0:X2}", (i == 0) ? 0 : (i * 4 - 1)));
				}
				DropDownColorSelector.options = list.ToArray();
			}
			dropDown.items = DropDownColorSelector.options;
			dropDown.eventSelectedIndexChanged += delegate(UIComponent component, int value)
			{
				this.m_displayColor.color = this.selectedColor;
				this.m_displayColor.text = string.Format("#{0:X2}{1:X2}{2:X2}", this.selectedColor.r, this.selectedColor.g, this.selectedColor.b);
				bool flag2 = this.eventColorChanged != null;
				if (flag2)
				{
					this.eventColorChanged(this.selectedColor);
				}
			};
			dropDown.useOutline = true;
			dropDown.outlineColor = Color.black;
			dropDown.textColor = Color.white;
			dropDown.width = 60f;
			dropDown.height = 28f;
			dropDown.itemPadding = new RectOffset(4, 4, 0, 0);
			dropDown.textFieldPadding = new RectOffset(5, 10, 5, 5);
			dropDown.focusedBgSprite = dropDown.normalBgSprite;
			dropDown.hoveredBgSprite = dropDown.normalBgSprite;
		}
		
		public void Destroy()
		{
			UnityEngine.Object.Destroy(this.m_parent.gameObject);
		}
	}
}

