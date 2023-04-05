using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Text;

public class Initializer : MonoBehaviour
{
	public static string fileName;
	public static int allEnemiesKilled;

    void Awake()
    {
		fileName = Application.persistentDataPath + @"/options.dat";
		if (!File.Exists(fileName)) { WriteDefaultValues(); }
		else { Debug.Log("The file already exists dummy"); }
		DisplayValues();

		//load next scene last
		SceneManager.LoadScene(1);
    }

	public static void WriteDefaultValues() {
		Debug.Log("No save file found, making a new one with default values");
		using (var stream = File.Open(fileName, FileMode.CreateNew)) {
			using (var writer = new BinaryWriter(stream, Encoding.UTF8, false)) {
				writer.Write(0);
			}
		}
	}

	public static void DisplayValues() {
		if (File.Exists(fileName)) {
			using (var stream = File.Open(fileName, FileMode.Open)) {
				using (var reader = new BinaryReader(stream, Encoding.UTF8, false)) {
					allEnemiesKilled = reader.ReadInt32();
				}
			}

			Debug.Log("(Initializer) Enemies killed: " + allEnemiesKilled);
		}

	}
}