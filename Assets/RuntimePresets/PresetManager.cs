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
	public void Start()
	{
		// Load last used preset if one exists
		PresetPath = Application.persistentDataPath + "/" + gameObject.name;
		if (Directory.Exists(PresetPath)) {
			var lastFile = Directory.GetFiles(PresetPath)
				.OrderByDescending(file => File.GetLastWriteTime(file))
				.FirstOrDefault();
			if (lastFile != null)
			{
				PresetName = lastFile.Substring(PresetPath.Length + 1).Replace(".bin", "");
				Debug.Log(PresetName);
				LoadPreset();
			}
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
		Controller.BeforeLoad();

		var formatter = new BinaryFormatter();
		var filePath = PresetPath + "/" +  PresetName + ".bin";
				Debug.Log(filePath);

		FileStream file = File.Open(filePath, FileMode.Open);
		Controller.Unpack(formatter.Deserialize(file) as List<RuntimeControllerParameter>);
		
		file.Close();

		Controller.AfterLoad();
	}
}