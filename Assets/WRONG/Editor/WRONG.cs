using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class WRONG 
{
	public static AudioSource source;
	public static AudioClip clip;
	public static bool active;


	public static void TriggerSound(string log, string stackTrace, LogType type){
		if(source == null){
			GameObject obj = new GameObject();
			obj.hideFlags = HideFlags.HideAndDontSave;
			source = obj.AddComponent<AudioSource>();
		}
		if(source.clip == null){
			source.clip = Resources.Load<AudioClip>("WRONG");
		}
		if(type == LogType.Error && active && !source.isPlaying){
			source.Play();
		}
	}


	static WRONG(){
		active = EditorPrefs.GetBool("AngryErrors");
		if(active){
			Application.logMessageReceived += TriggerSound;
		}
	}


}

class AngryErrorWindow : EditorWindow {
    [MenuItem ("Tools/Turn on Angry Errors")]
    public static void  Activate () {
		Application.logMessageReceived += WRONG.TriggerSound;
		EditorPrefs.SetBool("AngryErrors", true);
    }

	[MenuItem("Tools/Turn on Angry Errors", true)]
    static bool ValidateActivate()
    {
        return !EditorPrefs.GetBool("AngryErrors");
    }

    [MenuItem ("Tools/Turn off Angry Errors")]
    public static void  Deactivate () {
		Application.logMessageReceived -= WRONG.TriggerSound;
		EditorPrefs.SetBool("AngryErrors", false);
    }

	[MenuItem("Tools/Turn off Angry Errors", true)]
    static bool ValidateDeactivate()
    {
        return EditorPrefs.GetBool("AngryErrors");
    }
}
