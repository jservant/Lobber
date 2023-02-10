using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool debugTools = false;
    public Transform player;
    public GameObject enemy;
    DebugActions dActions;

    public List<Enemy> enemyList;

    void Awake() {
        player = transform.Find("/Player");
        if (player != null) {
            Debug.Log("Object Named Player found");
        } else Debug.LogWarning("Object Named Player Not found");

        enemy = Resources.Load("Prefabs/Enemies/Enemy") as GameObject;
        dActions = new DebugActions();
    }

    private void Update()
    {
        if (debugTools)
        {
            if (dActions.DebugTools.SpawnEnemy.WasPerformedThisFrame()) {
                Debug.Log("clig;");
                GameObject iEnemy = enemy;
                Vector3 mPos = Vector3.zero;
                Plane plane = new Plane(Vector3.up, 0);
                float distance;
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (plane.Raycast(ray, out distance))
                {
                    mPos = ray.GetPoint(distance);
                }
                //Vector3 heightCorrectedPoint = new Vector3(mPos.x, transform.position.y, mPos.z);
                //movement = heightCorrectedPoint
                Debug.Log("Mouse Look At point: " + mPos);
                Instantiate(iEnemy, new Vector3(mPos.x, mPos.y + 3, mPos.z), Quaternion.identity);
                //movement = heightCorrectedPoint; mayb for mouse attack dashing?
                //Debug.Log("heightCorrectedPoint: " + heightCorrectedPoint);
            }
        }
    }

    void OnEnable() { dActions.Enable(); }
    void OnDisable() { dActions.Disable(); }
}
