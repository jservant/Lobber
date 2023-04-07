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
	public static int versionNum;
	public static int allEnemiesKilled;
	public static int runsStarted;
	public static int timesWon;

	enum saveVersion { Init, Win, LATEST_PLUS_1 };

    void Awake()
    {
		fileName = Application.persistentDataPath + @"/options.dat";
		if (!File.Exists(fileName)) { WriteDefaultValues(); }
		else { Debug.Log("The file already exists dummy"); }
		Load();
		Save();

		//load next scene last
		SceneManager.LoadScene(1);
    }

	public static void WriteDefaultValues() {
		Debug.Log("No save file found, making a new one with default values");
		using (var stream = File.Open(fileName, FileMode.CreateNew)) {
			using (var writer = new BinaryWriter(stream, Encoding.UTF8, false)) {
				writer.Write((int)saveVersion.LATEST_PLUS_1 - 1);	//versionNum
				writer.Write(0);									//allEnemiesKilled
				writer.Write(0);									//timesGameBooted
				writer.Write(0);									//timesWon
			}
		}
	}

	public static void Load() {
		if (File.Exists(fileName)) {
			using (var stream = File.Open(fileName, FileMode.Open)) {
				using (var reader = new BinaryReader(stream, Encoding.UTF8, false)) {
					versionNum = reader.ReadInt32();
					if (versionNum == (int)saveVersion.Init) {
						allEnemiesKilled = reader.ReadInt32();
						runsStarted = reader.ReadInt32();
						timesWon = 0;
					}
					if (versionNum == (int)saveVersion.Win) {
						allEnemiesKilled = reader.ReadInt32();
						runsStarted = reader.ReadInt32();
						timesWon = reader.ReadInt32();
					}
				}
			}
			Debug.Log("(Initializer) Version num: " + versionNum);
			Debug.Log("(Initializer) Enemies killed: " + allEnemiesKilled);
			Debug.Log("(Initializer) Times game has been booted: " + runsStarted);
			Debug.Log("(Initializer) Times won: " + timesWon);
		}

	}

	public static void Save() {
		using (FileStream fs = new FileStream(fileName, FileMode.Create)) {
			using (BinaryWriter w = new BinaryWriter(fs)) {
				w.Write((int)saveVersion.LATEST_PLUS_1 - 1);
				w.Write(allEnemiesKilled);
				w.Write(runsStarted);
				w.Write(timesWon);
			}
		}
	}
}