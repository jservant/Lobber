using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices; 

public class Initializer : MonoBehaviour {
	public readonly static string fileName;

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
	public struct SaveFile_VersionTutorial {
		[FieldOffset(0)]  public int basicEnemyKills;
		[FieldOffset(4)]  public int explosiveEnemyKills;
		[FieldOffset(8)]  public int necroEnemyKills;
		[FieldOffset(12)] public int bruteEnemyKills;
		[FieldOffset(16)] public int runsStarted;
		[FieldOffset(20)] public int longestRun;
		[FieldOffset(24)] public int timesWon;
		[FieldOffset(28)] public bool tutorialComplete;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile_VersionMoreSettings {
		[FieldOffset(0)]  public int basicEnemyKills;
		[FieldOffset(4)]  public int explosiveEnemyKills;
		[FieldOffset(8)]  public int necroEnemyKills;
		[FieldOffset(12)] public int bruteEnemyKills;
		[FieldOffset(16)] public int runsStarted;
		[FieldOffset(20)] public int longestRun;
		[FieldOffset(24)] public int timesWon;
		[FieldOffset(28)] public bool tutorialComplete;
		[FieldOffset(32)] public float corpseLimit;
		[FieldOffset(36)] public float screenshakePercentage;
		[FieldOffset(40)] public bool rumble;
		[FieldOffset(44)] public float masterVolume;
		[FieldOffset(48)] public float musicVolume;
		[FieldOffset(52)] public float sfxVolume;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile_VersionResolutionSaver {
		[FieldOffset(0)]  public int basicEnemyKills;
		[FieldOffset(4)]  public int explosiveEnemyKills;
		[FieldOffset(8)]  public int necroEnemyKills;
		[FieldOffset(12)] public int bruteEnemyKills;
		[FieldOffset(16)] public int runsStarted;
		[FieldOffset(20)] public int longestRun;
		[FieldOffset(24)] public int timesWon;
		[FieldOffset(28)] public bool tutorialComplete;
		[FieldOffset(32)] public float corpseLimit;
		[FieldOffset(36)] public float screenshakePercentage;
		[FieldOffset(40)] public bool rumble;
		[FieldOffset(44)] public float masterVolume;
		[FieldOffset(48)] public float musicVolume;
		[FieldOffset(52)] public float sfxVolume;
		[FieldOffset(56)] public int resolutionOption;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile_VersionFOV {
		[FieldOffset(0)]  public int basicEnemyKills;
		[FieldOffset(4)]  public int explosiveEnemyKills;
		[FieldOffset(8)]  public int necroEnemyKills;
		[FieldOffset(12)] public int bruteEnemyKills;
		[FieldOffset(16)] public int runsStarted;
		[FieldOffset(20)] public int longestRun;
		[FieldOffset(24)] public int timesWon;
		[FieldOffset(28)] public bool tutorialComplete;
		[FieldOffset(32)] public float corpseLimit;
		[FieldOffset(36)] public float screenshakePercentage;
		[FieldOffset(40)] public bool rumble;
		[FieldOffset(44)] public float masterVolume;
		[FieldOffset(48)] public float musicVolume;
		[FieldOffset(52)] public float sfxVolume;
		[FieldOffset(56)] public int resolutionOption;
		[FieldOffset(60)] public float cameraFOV;
		[FieldOffset(64)] public bool hasCompletedCrystalTaskOnce;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile_VersionHardMode {
		[FieldOffset(0)]  public int basicEnemyKills;
		[FieldOffset(4)]  public int explosiveEnemyKills;
		[FieldOffset(8)]  public int necroEnemyKills;
		[FieldOffset(12)] public int bruteEnemyKills;
		[FieldOffset(16)] public int runsStarted;
		[FieldOffset(20)] public int longestRun;
		[FieldOffset(24)] public int timesWon;
		[FieldOffset(28)] public bool tutorialComplete;
		[FieldOffset(32)] public float corpseLimit;
		[FieldOffset(36)] public float screenshakePercentage;
		[FieldOffset(40)] public bool rumble;
		[FieldOffset(44)] public float masterVolume;
		[FieldOffset(48)] public float musicVolume;
		[FieldOffset(52)] public float sfxVolume;
		[FieldOffset(56)] public int resolutionOption;
		[FieldOffset(60)] public float cameraFOV;
		[FieldOffset(64)] public bool hasCompletedCrystalTaskOnce;
		[FieldOffset(68)] public bool hardModeUnlocked;
		[FieldOffset(72)] public float longestHardRun;
		[FieldOffset(76)] public float hardBasicEnemyKills;
		[FieldOffset(80)] public float hardExplosiveEnemyKills;
		[FieldOffset(84)] public float hardNecroEnemyKills;
		[FieldOffset(88)] public float hardBruteEnemyKills;
		[FieldOffset(92)] public float fireballsReflected;
		[FieldOffset(96)] public float headsCaught;
		[FieldOffset(100)] public float highestCombo;
		[FieldOffset(104)] public bool buttonsUI;
	}

	[StructLayout(LayoutKind.Explicit, Pack=8)]
	public struct SaveFile {
		[FieldOffset(0)] public int version;
		// NOTE(Roskuski): versionNum is our "Tag" for this "Tagged Union"

		[FieldOffset(4)] public SaveFile_VersionInit versionInit;
		[FieldOffset(4)] public SaveFile_VersionWin versionWin;
		[FieldOffset(4)] public SaveFile_VersionDiffKillCount versionDiffKillCount;
		[FieldOffset(4)] public SaveFile_VersionLongestRun versionLongestRun;
		[FieldOffset(4)] public SaveFile_VersionTutorial versionTutorial;
		[FieldOffset(4)] public SaveFile_VersionMoreSettings versionMoreSettings;
		[FieldOffset(4)] public SaveFile_VersionResolutionSaver versionResolutionSaver;
		[FieldOffset(4)] public SaveFile_VersionFOV versionFOV;
		[FieldOffset(4)] public SaveFile_VersionHardMode versionHardMode;
		// NOTE(Roskuski): Add additional versions here. at the same FieldOffset.

		// NOTE(Roskuski): Make sure this type stays in sync with the _actual_ latest version! Modify savedata though this variable
		[FieldOffset(4)] public SaveFile_VersionHardMode versionLatest;
	}

	public static SaveFile save;
	readonly static SaveFile DefaultSave;

	enum SaveVersion { Init, Win, DiffKillCount, LongestRun, Tutorial, MoreOptions, ResolutionSaver, FOV, HardMode, LATEST_PLUS_1 };

	static Initializer() {
        DefaultSave.version = (int)SaveVersion.LATEST_PLUS_1 - 1;
        DefaultSave.versionLatest.basicEnemyKills = 0;
        DefaultSave.versionLatest.explosiveEnemyKills = 0;
        DefaultSave.versionLatest.necroEnemyKills = 0;
        DefaultSave.versionLatest.bruteEnemyKills = 0;
        DefaultSave.versionLatest.runsStarted = 0;
        DefaultSave.versionLatest.longestRun = 0;
        DefaultSave.versionLatest.timesWon = 0;
        DefaultSave.versionLatest.tutorialComplete = false;
        DefaultSave.versionLatest.corpseLimit = 50;
        DefaultSave.versionLatest.screenshakePercentage = 100;
        DefaultSave.versionLatest.rumble = true;
        DefaultSave.versionLatest.masterVolume = 100;
        DefaultSave.versionLatest.musicVolume = 100;
        DefaultSave.versionLatest.sfxVolume = 100;
        DefaultSave.versionLatest.resolutionOption = 0;
        DefaultSave.versionLatest.cameraFOV = 60;
        DefaultSave.versionLatest.hasCompletedCrystalTaskOnce = false;
        DefaultSave.versionLatest.hardModeUnlocked = false;
        DefaultSave.versionLatest.longestHardRun = 0;
        DefaultSave.versionLatest.hardBasicEnemyKills = 0;
        DefaultSave.versionLatest.hardExplosiveEnemyKills = 0;
        DefaultSave.versionLatest.hardNecroEnemyKills = 0;
        DefaultSave.versionLatest.hardBruteEnemyKills = 0;
        DefaultSave.versionLatest.fireballsReflected = 0;
        DefaultSave.versionLatest.headsCaught = 0;
        DefaultSave.versionLatest.highestCombo = 0;
        DefaultSave.versionLatest.buttonsUI = false;

        fileName = Application.persistentDataPath + @"/options.dat";

		save = DefaultSave;

		if (!File.Exists(fileName)) {
			Save();
		}
		else {
			Load();
            AkSoundEngine.SetRTPCValue("MasterVolume", save.versionLatest.masterVolume);
            AkSoundEngine.SetRTPCValue("MusicVolume", save.versionLatest.musicVolume);
            AkSoundEngine.SetRTPCValue("SFXVolume", save.versionLatest.sfxVolume);
		}
	}

	public static void AssignDefaultValues() {
        save.versionLatest.basicEnemyKills = 0;
        save.versionLatest.explosiveEnemyKills = 0;
        save.versionLatest.necroEnemyKills = 0;
        save.versionLatest.bruteEnemyKills = 0;
        save.versionLatest.runsStarted = 0;
        save.versionLatest.longestRun = 0;
        save.versionLatest.timesWon = 0;
        save.versionLatest.tutorialComplete = false;
        save.versionLatest.corpseLimit = 50;
        save.versionLatest.screenshakePercentage = 100;
        save.versionLatest.rumble = true;
        save.versionLatest.masterVolume = 100;
        save.versionLatest.musicVolume = 100;
        save.versionLatest.sfxVolume = 100;
		save.versionLatest.cameraFOV = 60;
        save.versionLatest.hasCompletedCrystalTaskOnce = false;
        save.versionLatest.hardModeUnlocked = false;
        save.versionLatest.longestHardRun = 0;
        save.versionLatest.hardBasicEnemyKills = 0;
        save.versionLatest.hardExplosiveEnemyKills = 0;
        save.versionLatest.hardNecroEnemyKills = 0;
        save.versionLatest.hardBruteEnemyKills = 0;
        save.versionLatest.fireballsReflected = 0;
        save.versionLatest.headsCaught = 0;
        save.versionLatest.highestCombo = 0;
        save.versionLatest.buttonsUI = true;
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

			case (int)SaveVersion.LongestRun:
				save.versionTutorial.basicEnemyKills = loadedSave.versionLongestRun.basicEnemyKills;
				save.versionTutorial.explosiveEnemyKills = loadedSave.versionLongestRun.explosiveEnemyKills;
				save.versionTutorial.necroEnemyKills = loadedSave.versionLongestRun.necroEnemyKills;
				save.versionTutorial.bruteEnemyKills = loadedSave.versionLongestRun.bruteEnemyKills;
				save.versionTutorial.runsStarted = loadedSave.versionLongestRun.runsStarted;
				save.versionTutorial.timesWon = loadedSave.versionLongestRun.timesWon;
				loadedSave = save;
				goto case (int)SaveVersion.Tutorial;

			case (int)SaveVersion.Tutorial:
				save.versionMoreSettings.basicEnemyKills = loadedSave.versionTutorial.basicEnemyKills;
				save.versionMoreSettings.explosiveEnemyKills = loadedSave.versionTutorial.explosiveEnemyKills;
				save.versionMoreSettings.necroEnemyKills = loadedSave.versionTutorial.necroEnemyKills;
				save.versionMoreSettings.bruteEnemyKills = loadedSave.versionTutorial.bruteEnemyKills;
				save.versionMoreSettings.runsStarted = loadedSave.versionTutorial.runsStarted;
				save.versionMoreSettings.timesWon = loadedSave.versionTutorial.timesWon;
				loadedSave = save;
				goto case (int)SaveVersion.MoreOptions;

			case (int)SaveVersion.MoreOptions:
				save.versionResolutionSaver.basicEnemyKills = loadedSave.versionMoreSettings.basicEnemyKills;
				save.versionResolutionSaver.explosiveEnemyKills = loadedSave.versionMoreSettings.explosiveEnemyKills;
				save.versionResolutionSaver.necroEnemyKills = loadedSave.versionMoreSettings.necroEnemyKills;
				save.versionResolutionSaver.bruteEnemyKills = loadedSave.versionMoreSettings.bruteEnemyKills;
				save.versionResolutionSaver.runsStarted = loadedSave.versionMoreSettings.runsStarted;
				save.versionResolutionSaver.timesWon = loadedSave.versionMoreSettings.timesWon;
				save.versionResolutionSaver.longestRun = loadedSave.versionMoreSettings.longestRun;
				save.versionResolutionSaver.tutorialComplete = loadedSave.versionMoreSettings.tutorialComplete;
				loadedSave = save;
				goto case (int)SaveVersion.ResolutionSaver;

			case (int)SaveVersion.ResolutionSaver:
				save.versionFOV.basicEnemyKills = loadedSave.versionResolutionSaver.basicEnemyKills;
				save.versionFOV.explosiveEnemyKills = loadedSave.versionResolutionSaver.explosiveEnemyKills;
				save.versionFOV.necroEnemyKills = loadedSave.versionResolutionSaver.necroEnemyKills;
				save.versionFOV.bruteEnemyKills = loadedSave.versionResolutionSaver.bruteEnemyKills;
				save.versionFOV.runsStarted = loadedSave.versionResolutionSaver.runsStarted;
				save.versionFOV.timesWon = loadedSave.versionResolutionSaver.timesWon;
				save.versionFOV.longestRun = loadedSave.versionResolutionSaver.longestRun;
				save.versionFOV.tutorialComplete = loadedSave.versionResolutionSaver.tutorialComplete;
				save.versionFOV.corpseLimit = loadedSave.versionResolutionSaver.corpseLimit;
				save.versionFOV.screenshakePercentage = loadedSave.versionResolutionSaver.screenshakePercentage;
				save.versionFOV.rumble = loadedSave.versionResolutionSaver.rumble;
				save.versionFOV.masterVolume = loadedSave.versionResolutionSaver.masterVolume;
				save.versionFOV.musicVolume = loadedSave.versionResolutionSaver.musicVolume;
				save.versionFOV.sfxVolume = loadedSave.versionResolutionSaver.sfxVolume;
				save.versionFOV.resolutionOption = loadedSave.versionResolutionSaver.resolutionOption;
				loadedSave = save;
				goto case (int)SaveVersion.FOV;

			case (int)SaveVersion.FOV:
				save.versionHardMode.basicEnemyKills = loadedSave.versionFOV.basicEnemyKills;
				save.versionHardMode.explosiveEnemyKills = loadedSave.versionFOV.explosiveEnemyKills;
				save.versionHardMode.necroEnemyKills = loadedSave.versionFOV.necroEnemyKills;
				save.versionHardMode.bruteEnemyKills = loadedSave.versionFOV.bruteEnemyKills;
				save.versionHardMode.runsStarted = loadedSave.versionFOV.runsStarted;
				save.versionHardMode.timesWon = loadedSave.versionFOV.timesWon;
				save.versionHardMode.longestRun = loadedSave.versionFOV.longestRun;
				save.versionHardMode.tutorialComplete = loadedSave.versionFOV.tutorialComplete;
				save.versionHardMode.corpseLimit = loadedSave.versionFOV.corpseLimit;
				save.versionHardMode.screenshakePercentage = loadedSave.versionFOV.screenshakePercentage;
				save.versionHardMode.rumble = loadedSave.versionFOV.rumble;
				save.versionHardMode.masterVolume = loadedSave.versionFOV.masterVolume;
				save.versionHardMode.musicVolume = loadedSave.versionFOV.musicVolume;
				save.versionHardMode.sfxVolume = loadedSave.versionFOV.sfxVolume;
				save.versionHardMode.resolutionOption = loadedSave.versionFOV.resolutionOption;
				save.versionHardMode.cameraFOV = loadedSave.versionFOV.cameraFOV;
				save.versionHardMode.hasCompletedCrystalTaskOnce = loadedSave.versionFOV.hasCompletedCrystalTaskOnce;
				loadedSave = save;
				break;

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
