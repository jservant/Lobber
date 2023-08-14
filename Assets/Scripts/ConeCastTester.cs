using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ConeCastTester : MonoBehaviour
{
    public Vector3 targetSphereLocation;
    public float targetSphereDistance = 5f;
    public float tsr = 3f;
    public float coneAngle = 10f;
    public Renderer[] cubeRenderers;
    public Vector3[] coneWallPoints;

    private void Start() {
        cubeRenderers = transform.Find("/TestCubes").GetComponentsInChildren<Renderer>();
    }

    private void Update() {
        targetSphereLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z + targetSphereDistance);
        RaycastHit[] coneHits = Util.ConeCastAll(transform.position, tsr, Vector3.forward, targetSphereDistance + tsr, coneAngle);

        foreach (Renderer cube in cubeRenderers) {
            cube.material.color = Color.white;
            for (int i = 0; i < coneHits.Length; i++) {
                if (coneHits[i].collider.gameObject.name == cube.gameObject.name) {
                    cube.material.color = Color.blue;
                }
            }
        }

        coneWallPoints = new Vector3[4] {
            new Vector3(targetSphereLocation.x + tsr, targetSphereLocation.y, targetSphereLocation.z),
            new Vector3(targetSphereLocation.x - tsr, targetSphereLocation.y, targetSphereLocation.z),
            new Vector3(targetSphereLocation.x, targetSphereLocation.y + tsr, targetSphereLocation.z),
            new Vector3(targetSphereLocation.x, targetSphereLocation.y - tsr, targetSphereLocation.z)
        };

        if (Keyboard.current.rKey.wasPressedThisFrame) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red; // homing cone
        Gizmos.DrawWireSphere(targetSphereLocation, tsr);
        /*Gizmos.DrawLine(transform.position, new Vector3(targetSphereLocation.x + tsr, targetSphereLocation.y, targetSphereLocation.z));
        Gizmos.DrawLine(transform.position, new Vector3(targetSphereLocation.x - tsr, targetSphereLocation.y, targetSphereLocation.z));
        Gizmos.DrawLine(transform.position, new Vector3(targetSphereLocation.x, targetSphereLocation.y + tsr, targetSphereLocation.z));
        Gizmos.DrawLine(transform.position, new Vector3(targetSphereLocation.x, targetSphereLocation.y - tsr, targetSphereLocation.z));*/

        Gizmos.color = Color.blue; // bounds of the cone
        for (int i = 0; i < coneWallPoints.Length; i++) {
            Gizmos.DrawLine(transform.position, coneWallPoints[i]);
        }
        Gizmos.color = Color.yellow; // max homing distance
        Gizmos.DrawLine(transform.position, targetSphereLocation);
        //Gizmos.DrawLine(transform.position, new Vector3(targetSphereLocation.x, targetSphereLocation.y, targetSphereLocation.z + tsr));
    }
}
