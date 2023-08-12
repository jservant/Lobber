using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeRandomizer : MonoBehaviour
{
    public GameObject[] prop;
    public bool spawnProp;
    NodeManager nodeMan;
    GameManager gameMan;

    void Start()
    {
        nodeMan = GetComponentInParent<NodeManager>();
        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
        DisableAll();
        if (spawnProp) PickObject();
    }
    public void DisableAll() {
        foreach (GameObject g in prop) {
            if (g != null) g.SetActive(false);
        }
    }

    public void PickObject() {
        int random = 0;
        random = Random.Range(0, prop.Length);
        if (prop[random] != null) {
            if (CheckType(prop[random])) prop[random].SetActive(true);
        }
    }

    bool CheckType(GameObject thing) {
        var pile = thing.GetComponent<DestructibleProp>();
        var trap = thing.GetComponent<ExplosiveTrap>();

        if (pile != null && pile.canDropHeads) {
            if (pile.isHealthMachine == false) { //if it's a bone pile
                nodeMan.currentBonePiles += 1;
                if (nodeMan.currentBonePiles <= nodeMan.maxBonePiles) return true;
            }

            if (pile.isHealthMachine == true) { //if it's a slush Machine
                nodeMan.currentSlushMachines += 1;
                if (nodeMan.currentSlushMachines <= nodeMan.maxSlushMachines) return true;
            }

            else return false;
        }
        
        if (trap != null) {
            nodeMan.currentExplosiveTraps += 1;
            if (nodeMan.currentExplosiveTraps <= nodeMan.maxExplosiveTraps) return true;
            else return false;
        }

        if (pile == null && trap == null) return true;
        else return false;
    }
}
