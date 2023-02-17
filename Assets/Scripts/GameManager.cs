using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public Transform player;
    public PlayerController playerController;
    public GameObject enemy;
    DebugActions dActions;

    public Transform[] eSpawns;

    void Awake() {
        player = transform.Find("/Player");
        playerController = player.GetComponent<PlayerController>();
        if (player != null) {
            Debug.Log("Object Named Player found");
        } else Debug.LogWarning("Object Named Player Not found");

        enemy = Resources.Load("ActivePrefabs/Enemies/Enemy") as GameObject;
        dActions = new DebugActions();
    }

    private void Update() {
        // @TODO(Roskuski): This doesn't spawn enemies in the right spot
        if (dActions.DebugTools.SpawnEnemy.WasPerformedThisFrame()) { // TAKE THIS OUT IN FINAL RELEASE
            GameObject iEnemy = enemy;
            Transform spawn = eSpawns[Random.Range(0, eSpawns.Length)];
            Debug.Log(iEnemy.gameObject.name + " spawned at " + spawn);
            Instantiate(iEnemy, spawn.position, Quaternion.identity); //heightCorrectedPoint in middle

            /*Vector3 mPos = Vector3.zero;
            Plane plane = new Plane(Vector3.up, 0);
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (plane.Raycast(ray, out distance))
            {
                mPos = ray.GetPoint(distance);
            }
            Vector3 heightCorrectedPoint = new Vector3(mPos.x, transform.position.y, mPos.z);
            //movement = heightCorrectedPoint*/
        }
    }

    public void FreezeFrames(float frames) // NOTE(Ryan): Can be called to freeze the game for x number of frames
    {
        // TODO(Ryan): Might need bool here to tell GameManager when game is currently frozen / unfrozen
        var freezeDurationInSeconds = frames / 60f;
        StartCoroutine(Freeze(freezeDurationInSeconds));
    }

    IEnumerator Freeze(float duration)
    {
        var original = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = original;
    }

    void OnEnable() { dActions.Enable(); }
    void OnDisable() { dActions.Disable(); }
}
