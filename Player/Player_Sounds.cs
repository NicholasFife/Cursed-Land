using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Sounds : MonoBehaviour
{
    AudioSource aSource;

    [SerializeField]
    AudioClip shortSwish;
    [SerializeField]
    AudioClip longSwish;
    [SerializeField]
    AudioClip pullString;
    [SerializeField]
    AudioClip releaseString;


    // Start is called before the first frame update
    void Start()
    {
        aSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayShortSwish()
    { aSource.PlayOneShot(shortSwish); }

    public void PlayLongSwish()
    { aSource.PlayOneShot(longSwish); }

    public void PlayPullString()
    { aSource.PlayOneShot(pullString); }
    public void PlayReleaseString()
    { aSource.PlayOneShot(releaseString); }
}
