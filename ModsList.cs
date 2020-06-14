using System;
using System.IO;
using ICities;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ColossalFramework.Packaging;
using ColossalFramework.IO;
using System.Xml;
using System.Collections.Generic;
using ModsList.KlyteExtensions;

namespace ModsList {
	
	public class ModsList : LoadingExtensionBase, IUserMod {

		String modName = "Mods Listing";
		String version = "1.1.6";

		string modsListDirectory;
		string modsListSaveDirectoryPath;
		string modSettingsFilename;

		string extension = ".xml";

		public string Name {
			get { return modName; }
		}
		
		public string Description {
			get { return "Save and load activated mods, assets and district styles"; }
		}

		public ModsList() {
			modsListDirectory = Path.Combine (DataLocation.modsPath, "ModsList");
			modsListSaveDirectoryPath = Path.Combine(modsListDirectory, "ModsList_savefiles");
			modSettingsFilename = Path.Combine (modsListDirectory, "ModsListSettings.xml");

			xmlSettings.Encoding = System.Text.Encoding.Unicode;
			xmlSettings.Indent = true;
			xmlSettings.IndentChars = "\t";
		}
		
		public void checkDirectories () {
			if (!Directory.Exists (modsListDirectory))
				Directory.CreateDirectory (modsListDirectory);
			if (!Directory.Exists (modsListSaveDirectoryPath))
				Directory.CreateDirectory (modsListSaveDirectoryPath);
		}
		
		public void writeSettings() {

			checkDirectories ();

			XmlWriter xw = XmlWriter.Create(modSettingsFilename, xmlSettings);
			
			//header
			xw.WriteStartDocument ();
			xw.WriteComment ("Automatically generated XML file for cities : skyline's ModsList mod. Created by CoarxFlow.");
			xw.WriteStartElement ("ModsList");
			xw.WriteAttributeString ("version", version);

			xw.WriteStartElement("Settings");

			//config to load at start
			if (startApplyConfig != null && startApplyConfig.Length > 0) {
				xw.WriteStartElement("ApplyConfigOnStart");
				xw.WriteAttributeString ("name", startApplyConfig);
				xw.WriteEndElement();
			}

			//banned file names (which could not be deleted)
			foreach (string banned in bannedFiles) {
				xw.WriteStartElement("BannedConfig");
				xw.WriteAttributeString ("name", banned);
				xw.WriteEndElement();
			}

			xw.WriteStartElement ("TriggerEnableMods");
			xw.WriteAttributeString ("value", triggerEnableMods.ToString());
			xw.WriteEndElement ();

			xw.WriteStartElement ("LastSavedConfig");
			xw.WriteAttributeString ("name", lastConfigName);
			xw.WriteEndElement ();

			xw.WriteStartElement ("UnknownModsStatus");
			xw.WriteAttributeString ("value", unknownModsStatus);
			xw.WriteEndElement ();

			xw.WriteEndElement ();
			
			//close file
			xw.WriteEndElement ();
			xw.WriteEndDocument ();
			xw.Close ();
		}
	

		public void readSettings() {

			checkDirectories ();

			if (!File.Exists (modSettingsFilename))
				return;

			XmlReader xr = XmlReader.Create(modSettingsFilename);
			try {
				string text;
				bannedFiles.Clear ();
				startApplyConfig = "";
				while(xr.Read())
				{
					//detect useful tags
					if(xr.NodeType == XmlNodeType.Element && xr.HasAttributes)
					{
						switch (xr.Name)
						{
						case "ApplyConfigOnStart":
							text = xr.GetAttribute("name");
							if(text != null && text.Length > 0)
							{
								startApplyConfig = text;
							}
							break;
						case "BannedConfig":
							text = xr.GetAttribute("name");
							if(text != null && text.Length > 0)
							{
								bannedFiles.Add(text);
							}
							break;
						case "TriggerEnableMods" :
							text = xr.GetAttribute("value");
							if(text != null && text.Length > 0)
							{
								if(text.Equals("False"))
								{
									triggerEnableMods = false;
								}
								else{
									triggerEnableMods = true;
								}
							}
							break;
						case "UnknownModsStatus" :
							text = xr.GetAttribute("value");
							if(text != null && text.Length > 0)
							{
								unknownModsStatus = text;
							}
							break;
						case "LastSavedConfig" :
							text = xr.GetAttribute("name");
							if(text != null && text.Length > 0)
							{
								lastConfigName = text;
								configFilename = text + extension;
							}
							break;
						}
					}
				}

				xr.Close ();
			}
			catch(XmlException ex)
			{
				Debug.LogException(ex);
				xr.Close ();
				return;
			}
		}

		string lastConfigName = "My Config";
		string configFilename = "My Config.xml";
		public void changeConfigName(string name) {
			lastConfigName = name;
			if (!name.EndsWith (extension))
				name = name.Insert (name.Length, extension);
			else
				lastConfigName = lastConfigName.Substring (0, lastConfigName.Length - extension.Length);

			if (name.Length > extension.Length)
				configFilename = name;

		}

		XmlWriterSettings xmlSettings = new XmlWriterSettings();
		public void saveCurrentConfig() {

			checkDirectories ();

			XmlWriter xw = XmlWriter.Create(Path.Combine(modsListSaveDirectoryPath, configFilename), xmlSettings);

			//header
			xw.WriteStartDocument ();
			xw.WriteComment ("Automatically generated XML file for cities : skyline's ModsList mod. Created by CoarxFlow.");
			xw.WriteStartElement ("ModsList");
			xw.WriteAttributeString ("version", version);


			//list activated mods
			xw.WriteStartElement("Mods");
			foreach (PluginManager.PluginInfo current in Singleton<PluginManager>.instance.GetPluginsInfo())
			{
				try
				{
					IUserMod[] instances = current.GetInstances<IUserMod>();
					if (instances.Length >= 1 && !instances[0].Name.Equals(modName)) //protect against self referencing
					{
						xw.WriteStartElement("ModEntry");
						xw.WriteAttributeString("name", instances[0].Name);
						xw.WriteAttributeString("ID", current.publishedFileID.ToString());
						xw.WriteAttributeString("activated", current.isEnabled.ToString());
						xw.WriteEndElement();
					}
				}
				catch (UnityException ex)
				{
					Debug.LogException(ex);
					UIView.ForwardException(new ModException("A Mod caused an error", ex));
				}
				catch (Exception ex2)
				{
					Debug.LogException(ex2);
					UIView.ForwardException(new ModException("A Mod caused an error", ex2));
				}
			}
			xw.WriteEndElement ();
			
			//list activated assets
			xw.WriteStartElement("Assets");
			foreach (Package.Asset current in PackageManager.FilterAssets(new Package.AssetType[]
			                                                              {
				UserAssetType.CustomAssetMetaData
			}))
			{
				if (current.isMainAsset)
				{
					xw.WriteStartElement("AssetEntry");
					xw.WriteAttributeString("name", current.name);
					xw.WriteAttributeString("ID", current.package.GetPublishedFileID().ToString());
					xw.WriteAttributeString("activated", current.isEnabled.ToString());
					xw.WriteEndElement();
				}
			}
			xw.WriteEndElement ();

			//list activated district styles
			xw.WriteStartElement("DistrictStyles");
			foreach (Package.Asset current in PackageManager.FilterAssets(new Package.AssetType[]
			                                                              {
				UserAssetType.DistrictStyleMetaData
			}))
			{
				xw.WriteStartElement("DistrictStyleEntry");
				xw.WriteAttributeString("name", current.name);
				xw.WriteAttributeString("ID", current.package.GetPublishedFileID().ToString());
				xw.WriteAttributeString("activated", current.isEnabled.ToString ());
				xw.WriteEndElement();
			}
			xw.WriteEndElement ();

			//close file
			xw.WriteEndElement ();
			xw.WriteEndDocument ();
			xw.Close ();

			string shortened = configFilename.Substring (0, configFilename.Length - extension.Length);
			if (bannedFiles.Contains (shortened)) {
				bannedFiles.Remove (shortened);
				writeSettings ();
			}

			if(!creatingUI)
				updateConfigDropdown (shortened, -1);

			writeSettings ();
		}

		List<String> availableConfigs = new List<String> ();
		public void checkAvailableConfigs() {
			checkDirectories ();

			string[] xmls = Directory.GetFileSystemEntries(modsListSaveDirectoryPath, "*"+extension);
			availableConfigs.Clear ();
			foreach (string filename in xmls)
			{
				//check if file belongs to the mod
				XmlReader xr = XmlReader.Create(filename);
				if(xr.IsStartElement("ModsList"))
				{
					if(xr.GetAttribute("version").Contains("1.")) //version check, if ever needed
					{
						string configName = filename.Substring(modsListSaveDirectoryPath.Length+1, filename.Length-extension.Length-modsListSaveDirectoryPath.Length-1);
						if(!bannedFiles.Contains(configName))
							availableConfigs.Add(configName);
					}
				}
				xr.Close();
			}
		}

		int currentLoadOption = 0;
		public void changeLoadOption(int selection) {
			currentLoadOption = selection;
			
			if (currentLoadOption < 0 || currentLoadOption >= availableConfigs.Count)
				return;

			loadModsList (availableConfigs [currentLoadOption]);

			if (!creatingUI) {
				changeConfigName (availableConfigs [currentLoadOption]);
				configNameField.text = lastConfigName;

				updateListStats ();
				hideModsList ();
			}
		}

		public class Entry {

			/*public enum Type {
				Mod,
				Asset,
				DistrictStyle
			};

			Type type; */
			public bool activated;
			public string name;
			public string id;

			public Entry(bool act, string nam, string idd)
			{
				activated = act;
				name = nam;
				id = idd;
			}
		}
		
		List<Entry> configMods = new List<Entry> ();
		List<Entry> configAssets = new List<Entry> ();
		List<Entry> configDistrictStyles = new List<Entry> ();
		string unknownModsStatus = "";
		string[] unknownModsBehaviours = new string[] {"Do not change its status", "Activate it", "Deactivate it"};
		string readDocVersion;
		public void loadModsList(string configName) {

			configMods.Clear();
			configAssets.Clear();
			configDistrictStyles.Clear();
			
			string file = Path.Combine(modsListSaveDirectoryPath, configName + extension);
			
			if (!File.Exists (file))
				return;

			XmlReader xr = XmlReader.Create(file);;
			try {
				xr.IsStartElement ("ModsList");
				readDocVersion = xr.GetAttribute ("version");
				/*if (xr.Name == "ModsList" && xr.GetAttribute ("version") != null)
				switch (xr.GetAttribute ("version")) {
					case "1.01":
					loadModsListV101 (xr);
					break;
					case "1.1.0":
					loadModsListV110 (xr);
					break;
				}*/

				string mod_name;
				string mod_ID;
				string triggerStr;
				bool trigger;
				while(xr.Read())
				{
					//detect useful tags
					if(xr.NodeType == XmlNodeType.Element && xr.HasAttributes)
					{
						switch (xr.Name)
						{
						case "ModEntry":
							mod_name = xr.GetAttribute("name");
							mod_ID = xr.GetAttribute("ID");
							triggerStr = xr.GetAttribute("activated");
							if(mod_name == null || mod_name.Length == 0 || mod_ID == null || mod_ID.Length == 0)
							{
								break;
							}
							if(triggerStr == null)
							{
								trigger = true;
							}
							else
							{
								trigger = triggerStr.Equals("True");
							}
							configMods.Add(new Entry(trigger, mod_name, mod_ID));
							break;
						case "AssetEntry":
							mod_name = xr.GetAttribute("name");
							mod_ID = xr.GetAttribute("ID");
							triggerStr = xr.GetAttribute("activated");
							if(mod_name == null || mod_name.Length == 0 || mod_ID == null || mod_ID.Length == 0)
							{
								break;
							}
							if(triggerStr == null)
							{
								trigger = true;
							}
							else
							{
								trigger = triggerStr.Equals("True");
							}
							configAssets.Add(new Entry(trigger, mod_name, mod_ID));
							break;
						case "DistrictStyleEntry":
							mod_name = xr.GetAttribute("name");
							mod_ID = xr.GetAttribute("ID");
							triggerStr = xr.GetAttribute("activated");
							if(mod_name == null || mod_name.Length == 0 || mod_ID == null || mod_ID.Length == 0)
							{
								break;
							}
							if(triggerStr == null)
							{
								trigger = true;
							}
							else
							{
								trigger = triggerStr.Equals("True");
							}
							configDistrictStyles.Add(new Entry(trigger, mod_name, mod_ID));
							break;
						}
					}
				}

				xr.Close ();
			}
			catch(XmlException ex)
			{
				Debug.LogException(ex);
				xr.Close ();
				return;
			}

			if (!firstStart) {
				if (readDocVersion != null && readDocVersion.Equals ("1.01")) {
					docVersionField.text = "  Older Config version, only \n  activated mods and assets listed. \n  Resave to update.";
					docVersionField.textColor = Color.yellow;
				} else {
					docVersionField.text = "";
				}
			}
			
		}

		string startApplyConfig;
		public void applyOnRestart() {

			if (currentLoadOption < 0 || currentLoadOption >= availableConfigs.Count)
				return;
			
			startApplyConfig = availableConfigs [currentLoadOption];

			configRestartField.text = "  Config "+startApplyConfig+" will be applied on game restart.";

			writeSettings ();
		}

		const string MAX_ID = "18446744073709551615";
		public void applyConfig() {

			string name;
			string ID;
			bool found = false;
			//browse loaded mods
			if (firstStart || triggerEnableMods) {
				foreach (PluginManager.PluginInfo current in Singleton<PluginManager>.instance.GetPluginsInfo()) {
					string entryName = current.name;
					try {
						IUserMod[] instances = current.GetInstances<IUserMod> ();
						if (instances.Length >= 1) {
							name = instances [0].Name;
                            ID = current.publishedFileID.ToString();
							found = false;
							foreach(Entry entry in configMods)
							{
								if((entry.name.Equals(name) && entry.id.Equals(ID)) || (!entry.id.Equals(MAX_ID) && entry.id.Equals(ID)))
								{
									found = true;
									if(entry.activated)
									{
										Debug.Log ("ModsList : activate mod \"" + name + "\"");
										current.isEnabled = true;
									}
									else if(!name.Equals (modName)) //protect against self de-activation
									{
										Debug.Log ("ModsList : deactivate mod \"" + name + "\"");
										current.isEnabled = false;
									}
									break;
								}
							}
							if(!found && !name.Equals(modName)) //prevent self de-activation
							{
								if(unknownModsStatus.Equals(unknownModsBehaviours[1])) {
									current.isEnabled = true;
									Debug.Log ("ModsList : unlisted mod \"" + name + "\" activated");
								}
								else if(unknownModsStatus.Equals(unknownModsBehaviours[2])) {
									current.isEnabled = false;
									Debug.Log ("ModsList : unlisted mod \"" + name + "\" deactivated");
								}
								else {
									Debug.Log ("ModsList : unlisted mod \"" + name + "\" ignored");
								}
							}
						}
					} catch (UnityException ex) {
						Debug.LogException (ex);
						UIView.ForwardException (new ModException ("A Mod caused an error", ex));
					} catch (Exception ex2) {
						Debug.LogException (ex2);
						UIView.ForwardException (new ModException ("A Mod caused an error", ex2));
					}
				}
			}

			//browse loaded assets
			foreach (Package.Asset current in PackageManager.FilterAssets(new Package.AssetType[]
			                                                              {
				UserAssetType.CustomAssetMetaData
			}))
			{
				if(current.isMainAsset)
				{
					name = current.name;
					ID = current.package.GetPublishedFileID().ToString();
					found = false;
					foreach(Entry entry in configAssets)
					{
						if((entry.name.Equals(name) && entry.id.Equals(ID)) || (!entry.id.Equals(MAX_ID) && entry.id.Equals(ID)))
						{
							found = true;
							if(entry.activated)
							{
								Debug.Log ("ModsList : activate asset \"" + name + "\"");
								current.isEnabled = true;
							}
							else
							{
								Debug.Log ("ModsList : deactivate asset \"" + name + "\"");
								current.isEnabled = false;
							}
							break;
						}
					}
					if(!found)
					{
						if(unknownModsStatus.Equals(unknownModsBehaviours[1])) {
							current.isEnabled = true;
							Debug.Log ("ModsList : unlisted asset \"" + name + "\" activated");
						}
						else if(unknownModsStatus.Equals(unknownModsBehaviours[2])) {
							current.isEnabled = false;
							Debug.Log ("ModsList : unlisted asset \"" + name + "\" deactivated");
						}
						else {
							Debug.Log ("ModsList : unlisted asset \"" + name + "\" ignored");
						}
					}
				}
			}

			//browse loaded district styles
			foreach (Package.Asset current in PackageManager.FilterAssets(new Package.AssetType[]
			                                                              {
				UserAssetType.DistrictStyleMetaData
			}))
			{
				name = current.name;
				ID = current.package.GetPublishedFileID().ToString();
				found = false;
				foreach(Entry entry in configDistrictStyles)
				{
					if((entry.name.Equals(name) && entry.id.Equals(ID)) || (!entry.id.Equals(MAX_ID) && entry.id.Equals(ID)))
					{
						found = true;
						if(entry.activated)
						{
							Debug.Log ("ModsList : activate district style \"" + name + "\"");
							current.isEnabled = true;
						}
						else
						{
							Debug.Log ("ModsList : deactivate district style \"" + name + "\"");
							current.isEnabled = false;
						}
					}
				}
				if(!found)
				{
					if(unknownModsStatus.Equals(unknownModsBehaviours[1])) {
						current.isEnabled = true;
						Debug.Log ("ModsList : unlisted district style \"" + name + "\" activated");
					}
					else if(unknownModsStatus.Equals(unknownModsBehaviours[2])) {
						current.isEnabled = false;
						Debug.Log ("ModsList : unlisted district style \"" + name + "\" deactivated");
					}
					else {
						Debug.Log ("ModsList : unlisted district style \"" + name + "\" ignored");
					}
				}
			}

			if (!firstStart) {
				configAppliedField.text = "  Config " + availableConfigs [currentLoadOption] + " successfully applied.";
				configRestartField.text = "";
				//force assets UI update
				PackageManager.ForcePackagesChanged ();
			} else {
				configRestartField.text = "  Config "+startApplyConfig+" has been applied.";
			}
		}

		public List<string> bannedFiles = new List<string> ();
		public void deleteConfig() {
			if (currentLoadOption < 0 || currentLoadOption >= availableConfigs.Count)
				return;

			try {
				File.Delete(Path.Combine(modsListSaveDirectoryPath, availableConfigs[currentLoadOption] + extension));
			}
			catch (IOException ex)
			{
				bannedFiles.Add(availableConfigs[currentLoadOption]);
			}

			writeSettings ();

			if(!creatingUI)
				updateConfigDropdown (null, currentLoadOption);
		}

		bool triggerEnableMods = false;
		public void changeLoadModOption(bool check) {
			triggerEnableMods = !check;

			writeSettings ();
		}
		
		public void changeUnknownModBehaviour(int val) {
			if (val < 0 || val >= unknownModsBehaviours.Length) {
				val = 0;
			}
			unknownModsStatus = unknownModsBehaviours[val];

			writeSettings ();
		}
	

		UIDropDown configDropdown;
		UIHelperExtension helperExtension;
		UIHelperExtension modsList;
		UIHelperExtension assetsList;
		UIHelperExtension districtsList;
		UITextField configNameField;
		UILabel docVersionField;
		UILabel configRestartField;
		UILabel configAppliedField;
		UIButton listButton;
		UILabel statsField;
		bool firstStart = true;
		bool creatingUI = false;
		public void OnSettingsUI(UIHelperBase helper)
		{

			helperExtension = new UIHelperExtension((UIHelper) helper);

//			if (Singleton<SimulationManager>.instance.m_metaData != null) {
//				helperExtension.AddLabel ("Please use from Content Manager only!");
//			} else {
				creatingUI = true;

				if (firstStart) {
					readSettings ();
				}


				checkAvailableConfigs ();

				UIHelperExtension groupSave = helperExtension.AddGroupExtended ("Save currently activated mods, assets and district styles");
				((UIPanel)groupSave.self).autoLayoutDirection = LayoutDirection.Horizontal;
				((UIPanel)groupSave.self).wrapLayout = true;
				((UIPanel)groupSave.self).width = 730;
				configNameField = (UITextField)groupSave.AddTextfield ("Enter name", lastConfigName, changeConfigName, changeConfigName);
				groupSave.AddButton ("Save", saveCurrentConfig);

				helperExtension.AddSpace (20);

				UIHelperExtension groupLoad = helperExtension.AddGroupExtended ("Load a previously saved configuration");
				((UIPanel)groupLoad.self).autoLayoutDirection = LayoutDirection.Horizontal;
				((UIPanel)groupLoad.self).wrapLayout = true;
				((UIPanel)groupLoad.self).width = 700;
				configDropdown = (UIDropDown)groupLoad.AddDropdown ("Pick config", availableConfigs.ToArray (), currentLoadOption, changeLoadOption);
				groupLoad.AddButton ("Delete", deleteConfig);
				docVersionField = groupLoad.AddLabel ("");
				groupLoad.AddSpace (20);
				groupLoad.AddButton ("Apply On Game Restart", applyOnRestart);
				configRestartField = groupLoad.AddLabel ("");
				configRestartField.textColor = Color.cyan;
				groupLoad.AddSpace (20);
				UIButton applyNow = (UIButton) groupLoad.AddButton ("Apply Now", applyConfig);
				configAppliedField = groupLoad.AddLabel ("");
				if (Singleton<SimulationManager>.instance.m_metaData != null) {
					applyNow.Disable ();
					configAppliedField.text = "Available from Content Manager Only";
					configAppliedField.textColor = Color.grey;
				} else {
					configAppliedField.textColor = Color.cyan;
				}
				groupLoad.AddCheckbox ("Do not affect mods status", !triggerEnableMods, changeLoadModOption);
				groupLoad.AddLabel ("If you have many mods, applying the configuration immediately will cause \na long freeze. The game will be also slowed down after that. \nYou can however choose to affect the assets and district styles status only, \nwhich does not cause any perfomance issue.");
				groupLoad.AddSpace (70);
				groupLoad.AddDropdown ("When finding an unlisted mod/asset :", unknownModsBehaviours, unknownModsStatus, changeUnknownModBehaviour).width = 400;

				helperExtension.AddSpace (20);

				UIHelperExtension groupListing = helperExtension.AddGroupExtended ("Listing");
				((UIPanel)groupListing.self).autoLayoutDirection = LayoutDirection.Horizontal;
				((UIPanel)groupListing.self).wrapLayout = true;
				((UIPanel)groupListing.self).width = 730;
				((UIPanel)groupListing.self.parent).Find<UILabel> ("Label").isVisible = false;
				statsField = groupListing.AddLabel ("");
				groupListing.AddSpace (10);
				listButton = (UIButton)groupListing.AddButton ("View List (slow)", updateModsList);
				groupListing.AddButton ("Copy List To Clipboard", configToClipboard);
				groupListing.AddSpace (10);

				modsList = helperExtension.AddGroupExtended ("Mods Listed");
				((UIPanel)modsList.self).autoLayoutDirection = LayoutDirection.Horizontal;
				((UIPanel)modsList.self).wrapLayout = true;
				((UIPanel)modsList.self).width = 730;
			
				assetsList = helperExtension.AddGroupExtended ("Assets Listed");
				((UIPanel)assetsList.self).autoLayoutDirection = LayoutDirection.Horizontal;
				((UIPanel)assetsList.self).wrapLayout = true;
				((UIPanel)assetsList.self).width = 730;
			
				districtsList = helperExtension.AddGroupExtended ("District Styles Listed");
				((UIPanel)districtsList.self).autoLayoutDirection = LayoutDirection.Horizontal;
				((UIPanel)districtsList.self).wrapLayout = true;
				((UIPanel)districtsList.self).width = 730;

				//apply restart config config
				if (firstStart) {
					if (startApplyConfig != null && startApplyConfig.Length > 0) {
						loadModsList (startApplyConfig);
						applyConfig ();
					}
					startApplyConfig = "";
				
					writeSettings ();
				
					firstStart = false;
				}

				creatingUI = false;


				//load last saved config
				int lastIndex = availableConfigs.IndexOf (lastConfigName);

				if (lastIndex >= 0) {
					changeLoadOption (lastIndex);
					configDropdown.selectedIndex = lastIndex;
				} else
					changeLoadOption (0);
			//}

		}

		public void updateListStats()
		{
			statsField.text = configMods.Count+" mods, "+configAssets.Count+" assets and "+configDistrictStyles.Count+" district styles listed.";
		}
		
		bool listDisplayed = false;
		string lastDisplayedConfig = "";
		const int LABEL_HEIGHT = 12;
		public void updateModsList()
		{
			if(!listDisplayed && !creatingUI) {

				((UIPanel)modsList.self.parent).Find<UILabel> ("Label").isVisible = true;
				modsList.self.isVisible = true;
				
				((UIPanel)assetsList.self.parent).Find<UILabel> ("Label").isVisible = true;
				assetsList.self.isVisible = true;
				
				((UIPanel)districtsList.self.parent).Find<UILabel> ("Label").isVisible = true;
				districtsList.self.isVisible = true;

				if(!lastDisplayedConfig.Equals(availableConfigs[currentLoadOption])) //if toggle button without changing load option, do not delete list
				{

					foreach (UIComponent comp in modsList.self.GetComponentsInChildren<UIComponent>()) {
					if(!comp.name.Equals("Content"))
						GameObject.Destroy(comp.gameObject);
					}
					modsList.self.components.Clear ();

					foreach (UIComponent comp in assetsList.self.GetComponentsInChildren<UIComponent>()) {
						if(!comp.name.Equals("Content"))
							GameObject.Destroy(comp.gameObject);
					}
					assetsList.self.components.Clear ();

					foreach (UIComponent comp in districtsList.self.GetComponentsInChildren<UIComponent>()) {
						if(!comp.name.Equals("Content"))
							GameObject.Destroy(comp.gameObject);
					}
					districtsList.self.components.Clear ();


					string status;
					Color color;
					//list mods status
					foreach (Entry entry in configMods) {
						UILabel label = modsList.AddLabel (entry.name);

						if (entry.activated) {
							status = "activated";
							color = Color.green;
						} else {
							status = "deactivated";
							color = Color.red;
						}

						UILabel lab1 = modsList.AddLabel ("");
						UILabel lab2 = modsList.AddLabel (status);
						lab2.textColor = color;
					}

					foreach (UIComponent comp in modsList.self.GetComponentsInChildren<UIComponent>()) {
						if (!comp.name.Equals ("Content"))
							comp.height = LABEL_HEIGHT;
					}

					//list assets status
					foreach (Entry entry in configAssets) {
						UILabel label = assetsList.AddLabel (entry.name);
					
						if (entry.activated) {
							status = "activated";
							color = Color.green;
						} else {
							status = "deactivated";
							color = Color.red;
						}

						assetsList.AddLabel ("");
						assetsList.AddLabel (status).textColor = color;
					}

					foreach (UIComponent comp in assetsList.self.GetComponentsInChildren<UIComponent>()) {
						if (!comp.name.Equals ("Content"))
							comp.height = LABEL_HEIGHT;
					}

					//list assets status
					foreach (Entry entry in configDistrictStyles) {
						UILabel label = districtsList.AddLabel (entry.name);
					
						if (entry.activated) {
							status = "activated";
							color = Color.green;
						} else {
							status = "deactivated";
							color = Color.red;
						}
					
						districtsList.AddLabel ("");
						districtsList.AddLabel (status).textColor = color;
					}

					foreach (UIComponent comp in districtsList.self.GetComponentsInChildren<UIComponent>()) {
						if (!comp.name.Equals ("Content"))
							comp.height = LABEL_HEIGHT;
					}
				}

				listDisplayed = true;
				listButton.text = "Hide List";
				lastDisplayedConfig = availableConfigs[currentLoadOption];
			}
			else {
				hideModsList();
			}
		}

		public void hideModsList()
		{
			if (modsList == null)
				return;

			UILabel label = ((UIPanel)modsList.self.parent).Find<UILabel> ("Label");
			if (label == null)
				return;

			label.isVisible = false;
			modsList.self.isVisible = false;

			if (assetsList == null)
				return;
			
			label = ((UIPanel)assetsList.self.parent).Find<UILabel> ("Label");
			if (label == null)
				return;

			label.isVisible = false;
			assetsList.self.isVisible = false;

			if (districtsList == null)
				return;
			
			label = ((UIPanel)districtsList.self.parent).Find<UILabel> ("Label");
			if (label == null)
				return;

			label.isVisible = false;
			districtsList.self.isVisible = false;

			listDisplayed = false;

			if (listButton == null)
				return;

			listButton.text = "View List (slow)";
		}

		const int ALIGN_AT_NCHARS = 60;
		public void configToClipboard()
		{
			StringWriter sw = new StringWriter();


			for (int i = 0; i < ALIGN_AT_NCHARS; i++) {
				sw.Write(' ');
			}
			string spaces = sw.ToString();
			sw = new StringWriter ();

			sw.WriteLine ("config " + availableConfigs[currentLoadOption]);
			sw.WriteLine ();

			sw.WriteLine ("### mods list ###");
			sw.WriteLine ();
			string status;
			int appendSpace;
			string nameID;
			foreach (Entry entry in configMods)
			{

				if(entry.activated)
				{
					status = "activated";
				}
				else
				{
					status = "deactivated";
				}

				nameID = entry.name + " (ID="+entry.id+")";
				appendSpace = ALIGN_AT_NCHARS-nameID.Length;
				if(appendSpace < 1)
					appendSpace = 1;
				sw.WriteLine(nameID+spaces.Substring(0,appendSpace)+status);
			}

			sw.WriteLine ();
			sw.WriteLine ("### assets list ###");
			sw.WriteLine ();
			foreach (Entry entry in configAssets)
			{
				if(entry.activated)
				{
					status = "activated";
				}
				else
				{
					status = "deactivated";
				}
				
				nameID = entry.name + " (ID="+entry.id+")";
				appendSpace = ALIGN_AT_NCHARS-nameID.Length;
				if(appendSpace < 1)
					appendSpace = 1;
				sw.WriteLine(nameID+spaces.Substring(0,appendSpace)+status);
			}

			sw.WriteLine ();
			sw.WriteLine ("### district styles list ###");
			sw.WriteLine ();
			foreach (Entry entry in configDistrictStyles) {
				if (entry.activated) {
					status = "activated";
				} else {
					status = "deactivated";
				}
				
				nameID = entry.name + " (ID="+entry.id+")";
				appendSpace = ALIGN_AT_NCHARS-nameID.Length;
				if(appendSpace < 1)
					appendSpace = 1;
				sw.WriteLine(nameID+spaces.Substring(0,appendSpace)+status);
			}


			Clipboard.text = sw.ToString();
			sw.Dispose ();
		}

		public void updateConfigDropdown(string selectConfig, int selectIndex)
		{
			checkAvailableConfigs ();

			configDropdown.items = availableConfigs.ToArray();

			if (currentLoadOption >= availableConfigs.Count)
				currentLoadOption = availableConfigs.Count - 1;
			else if (currentLoadOption < 0)
				currentLoadOption = 0;

			if (selectConfig != null)
				configDropdown.selectedIndex = availableConfigs.IndexOf (selectConfig);
			else {
				if(selectIndex >= availableConfigs.Count)
					selectIndex = availableConfigs.Count-1;
				else if(selectIndex < 0)
					selectIndex = 0;
				configDropdown.selectedIndex = selectIndex;
			}

			updateListStats ();
			hideModsList ();
		}
		
//		public override void OnLevelLoaded(LoadMode mode)
//		{
//			base.OnLevelLoaded(mode);
//			
//			Debug.Log ("ModsList Loaded");
//		}
//
//		public override void OnCreated(ILoading loading)
//		{
//			base.OnCreated (loading);
//			
//			Debug.Log ("ModsList creation.");
//
//		}
		
	}
}
