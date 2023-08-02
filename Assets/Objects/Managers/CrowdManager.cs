using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    public AK.Wwise.Event[] crowdSounds;

    // Start is called before the first frame update
    void Start()
    {
        PlayCrowdSound(1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayCrowdSound(int id) {
        crowdSounds[id].Post(gameObject);
    }

    public void StopCrowdSound() {
        crowdSounds[0].Post(gameObject);
    }

    private void OnDestroy() {
        StopCrowdSound();
    }
}
