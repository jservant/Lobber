using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices; 

public class Initializer : MonoBehaviour {
	public static string fileName;

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile_VersionInit {
		[FieldOffset(0)] public int allEnemiesKilled;
		[FieldOffset(4)] public int runsStarted;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile_VersionWin {
		[FieldOffset(0)] public int allEnemiesKilled;
		[FieldOffset(4)] public int runsStarted;
		[FieldOffset(8)] public int timesWon;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile_VersionDiffKillCount {
		[FieldOffset(0)]  public int basicEnemyKills;
		[FieldOffset(4)]  public int explosiveEnemyKills;
		[FieldOffset(8)]  public int necroEnemyKills;
		[FieldOffset(12)] public int bruteEnemyKills;
		[FieldOffset(16)] public int runsStarted;
		[FieldOffset(20)] public int timesWon;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile_VersionLongestRun {
		[FieldOffset(0)]  public int basicEnemyKills;
		[FieldOffset(4)]  public int explosiveEnemyKills;
		[FieldOffset(8)]  public int necroEnemyKills;
		[FieldOffset(12)] public int bruteEnemyKills;
		[FieldOffset(16)] public int runsStarted;
		[FieldOffset(20)] public int longestRun;
		[FieldOffset(24)] public int timesWon;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile {
		[FieldOffset(0)] public int version;
		// NOTE(Roskuski): versionNum is our "Tag" for this "Tagged Union"

		[FieldOffset(4)] public SaveFile_VersionInit versionInit;
		[FieldOffset(4)] public SaveFile_VersionWin versionWin;
		[FieldOffset(4)] public SaveFile_VersionDiffKillCount versionDiffKillCount;
		[FieldOffset(4)] public SaveFile_VersionLongestRun versionLongestRun;
		// NOTE(Roskuski): Add additional versions here. at the same FieldOffset.

		// NOTE(Roskuski): Make sure this type stays in sync with the _actual_ latest version! Modify savedata though this variable
		[FieldOffset(4)] public SaveFile_VersionLongestRun versionLatest;
	}

	public static SaveFile save;
	readonly static SaveFile DefaultSave;

	enum SaveVersion { Init, Win, DiffKillCount, LongestRun, LATEST_PLUS_1 };

	static Initializer() {
		DefaultSave.version = (int)SaveVersion.LATEST_PLUS_1 - 1;
		DefaultSave.versionLatest.basicEnemyKills = 0;
		DefaultSave.versionLatest.explosiveEnemyKills = 0;
		DefaultSave.versionLatest.necroEnemyKills = 0;
		DefaultSave.versionLatest.bruteEnemyKills = 0;
		DefaultSave.versionLatest.runsStarted = 0;
		DefaultSave.versionLatest.longestRun = 0;
		DefaultSave.versionLatest.timesWon = 0;
	}

	void Awake() {
		fileName = Application.persistentDataPath + @"/options.dat";
		save = DefaultSave;

		if (!File.Exists(fileName)) {
			Save();
		}
		else {
			Load();
		}

		//load next scene last
		SceneManager.LoadScene((int)Scenes.MainMenu);
	}

	public static void Load() {
		SaveFile loadedSave = DefaultSave;

		using (var stream = File.Open(fileName, FileMode.Open)) {
			using (var reader = new BinaryReader(stream, Encoding.UTF8, false)) {
				byte[] rawSave = reader.ReadBytes(Marshal.SizeOf(save));
				GCHandle rawSaveHandle = GCHandle.Alloc(rawSave, GCHandleType.Pinned);
				loadedSave = (SaveFile)Marshal.PtrToStructure(rawSaveHandle.AddrOfPinnedObject(), typeof(SaveFile));
				rawSaveHandle.Free();
			}
		}

		// NOTE(Roskuski): Upversion savefiles.
		switch (loadedSave.version) {
			default: 
				if (loadedSave.version >= (int)SaveVersion.LATEST_PLUS_1) {
					// NOTE(Roskuski): Unlikely that we'll encounter a savefile from the future, but incase we do...
					save = DefaultSave;
				}
				else {
					Debug.Assert(false, "I don't know how to upversion this save! (" + loadedSave.version + ")");
				}
				break;

				// NOTE(Roskuski): The following code incrementally upversions the save file.
				// save is used as a "scratch" save, while loadedSave is used as a "reference" while updating.
			case (int)SaveVersion.Init:
				save.versionWin.runsStarted = loadedSave.versionInit.runsStarted;
				save.versionWin.timesWon = DefaultSave.versionLatest.timesWon;
				loadedSave = save;
				goto case (int)SaveVersion.Win; // NOTE(Roskuski): c# is an annoying language. Why can't there be a "fallthough" keyword

			case (int)SaveVersion.Win:
				save.versionDiffKillCount.runsStarted = loadedSave.versionWin.runsStarted;
				save.versionDiffKillCount.timesWon = loadedSave.versionWin.timesWon;
				loadedSave = save;
				goto case (int)SaveVersion.DiffKillCount;

			case (int)SaveVersion.DiffKillCount:
				save.versionLongestRun.basicEnemyKills = loadedSave.versionDiffKillCount.basicEnemyKills;
				save.versionLongestRun.explosiveEnemyKills = loadedSave.versionDiffKillCount.explosiveEnemyKills;
				save.versionLongestRun.necroEnemyKills = loadedSave.versionDiffKillCount.necroEnemyKills;
				save.versionLongestRun.bruteEnemyKills = loadedSave.versionDiffKillCount.bruteEnemyKills;
				save.versionLongestRun.runsStarted = loadedSave.versionDiffKillCount.runsStarted;
				save.versionLongestRun.timesWon = loadedSave.versionDiffKillCount.timesWon;
				loadedSave = save;
				goto case (int)SaveVersion.LongestRun;

			case (int)SaveVersion.LATEST_PLUS_1 - 1: // NOTE(Roskuski): Latest version never needs to be converted.
				save = loadedSave;
				break;
		}
	}

	public static void Save() {
		using (FileStream fs = new FileStream(fileName, FileMode.Create)) {
			using (BinaryWriter w = new BinaryWriter(fs)) {
				int saveLength = Marshal.SizeOf(save);
				byte[] rawSave = new byte[saveLength];
				GCHandle saveHandle = GCHandle.Alloc(save, GCHandleType.Pinned);
				Marshal.Copy(saveHandle.AddrOfPinnedObject(), rawSave, 0, saveLength);
				w.Write(rawSave);
				saveHandle.Free();
			}
		}
	}
}
