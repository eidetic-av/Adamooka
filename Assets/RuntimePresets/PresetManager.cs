using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeInspectorNamespace;
using System;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Eidetic.Unity.Runtime;

public class PresetManager : MonoBehaviour {

	public string PresetName;
	string PresetPath;
	List<IEnumerable<RuntimeControllerParameter>> AvailablePresets;
	protected RuntimeController Controller;

	public static void Instantiate(RuntimeController parentController)
	{
		var presetManager = parentController.gameObject.AddComponent<PresetManager>();
		presetManager.Controller = parentController;
		presetManager.PresetName = DateTime.Now.ToFileTime().ToString();
	}
	public void Awake()
	{
		// Load presets if they are present in folder
		PresetPath = Application.persistentDataPath + "/" + gameObject.name;
		if (Directory.Exists(PresetPath)) {
			var files = Directory.GetFiles(PresetPath);
		}
	}


	[RuntimeInspectorButton("Save Preset", false, ButtonVisibility.InitializedObjects)]
	public void SavePreset()
	{
		var formatter = new BinaryFormatter();
		var filePath = PresetPath + "/" +  PresetName + ".bin";

		if (!Directory.Exists(PresetPath)) 
			Directory.CreateDirectory(PresetPath);

		FileStream file = File.Open(filePath, FileMode.OpenOrCreate);
		formatter.Serialize(file, Controller.Pack());
		file.Close();

		Debug.Log("Saved preset to: " + filePath);
	}


	[RuntimeInspectorButton("Load Preset", false, ButtonVisibility.InitializedObjects)]
	public void LoadPreset()
	{
		var formatter = new BinaryFormatter();
		var filePath = PresetPath + "/" +  PresetName + ".bin";

		FileStream file = File.Open(filePath, FileMode.Open);
		Controller.Unpack(formatter.Deserialize(file) as List<RuntimeControllerParameter>);
		
		file.Close();
	}
}