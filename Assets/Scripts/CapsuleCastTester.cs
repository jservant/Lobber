using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CapsuleCastTester : MonoBehaviour
{
    public Vector3 capsuleEndPosition;
    public float capsuleLength = 5f;
    public float capsuleRadius = 3f;
    public float homingTargetDeltaCap = 10f;
    public Renderer[] cubeRenderers;

    private void Start() {
        cubeRenderers = transform.Find("/TestCubes").GetComponentsInChildren<Renderer>();
    }

    private void Update() {
        capsuleEndPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + capsuleLength);
        RaycastHit[] capsuleHits = Physics.CapsuleCastAll(transform.position, capsuleEndPosition, capsuleRadius, Vector3.forward, 0, Mask.Get(Layers.EnemyHurtbox));

        foreach (Renderer cube in cubeRenderers) {
            cube.material.color = Color.white;
            for (int i = 0; i < capsuleHits.Length; i++) {
                if (capsuleHits[i].collider.gameObject.name == cube.gameObject.name) {
                    cube.material.color = Color.blue;
                }
            }
        }

        Vector3 homingTargetDelta = Vector3.forward * homingTargetDeltaCap;
        int target = 0;
        for (int index = 0; index < capsuleHits.Length; index += 1) {                                // for every collider found...
            Vector3 distanceDelta = capsuleHits[index].transform.position - capsuleEndPosition;      // calculate the delta between player and the enemy collider
            if (distanceDelta.magnitude < homingTargetDelta.magnitude) {                            // if current delta is lower than the previous one...
                homingTargetDelta = distanceDelta;                                                  // make it the new delta
                target = index;
            }
        }

        capsuleHits[target].collider.GetComponent<Renderer>().material.color = Color.red;

        if (Keyboard.current.rKey.wasPressedThisFrame) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red; // base spheres of the capsule
        Gizmos.DrawWireSphere(transform.position, capsuleRadius);
        Gizmos.DrawWireSphere(capsuleEndPosition, capsuleRadius);

        Gizmos.color = Color.blue; // bounds of the capsule
        Gizmos.DrawLine(new Vector3(transform.position.x + capsuleRadius, transform.position.y, transform.position.z), new Vector3(capsuleEndPosition.x + capsuleRadius, capsuleEndPosition.y, capsuleEndPosition.z));
        Gizmos.DrawLine(new Vector3(transform.position.x - capsuleRadius, transform.position.y, transform.position.z), new Vector3(capsuleEndPosition.x - capsuleRadius, capsuleEndPosition.y, capsuleEndPosition.z));
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y + capsuleRadius, transform.position.z), new Vector3(capsuleEndPosition.x, capsuleEndPosition.y + capsuleRadius, capsuleEndPosition.z));
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y - capsuleRadius, transform.position.z), new Vector3(capsuleEndPosition.x, capsuleEndPosition.y - capsuleRadius, capsuleEndPosition.z));

        Gizmos.color = Color.yellow; // length + radius of capsule
        Gizmos.DrawLine(transform.position, capsuleEndPosition);
        Gizmos.DrawLine(capsuleEndPosition, new Vector3(capsuleEndPosition.x + capsuleRadius, capsuleEndPosition.y, capsuleEndPosition.z));
    }
}
