using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using GoogleARCore;

public class AugmentedImageVisualizerScript : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public AugmentedImage image;
    [SerializeField]
    private VideoClip[] videoClip;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnStop;
    }

    private void OnStop(VideoPlayer source)
    {
        gameObject.SetActive(false);
    }

    void Update() 
    {
        if (image ==null || image.TrackingState != TrackingState.Tracking)
        {
            return;
        }

        if (!videoPlayer.isPlaying)
        {
            videoPlayer.clip = videoClip[image.DatabaseIndex]; //  
            videoPlayer.Play();
        }

        transform.localScale = new Vector3(image.ExtentX, image.ExtentZ, 1);
    }
}
