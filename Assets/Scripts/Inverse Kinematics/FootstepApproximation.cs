using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepApproximation : MonoBehaviour
{
    public AudioSource FootstepSource;
    public AudioClip FootstepSFX;
    
    private void Step()
    {
        FootstepSource.PlayOneShot(FootstepSFX);
    }
}
