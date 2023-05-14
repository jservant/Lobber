using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FindBooth : MonoBehaviour
{
    public CinemachineVirtualCamera VCam;
    public CinemachineTransposer cineTrnasposer;
    public Transform Booth;
    // Start is called before the first frame update
    void Start()
    {
        Booth = transform.Find("/NecroBooth");
        VCam.LookAt = Booth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
