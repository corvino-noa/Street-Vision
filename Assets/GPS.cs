using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GPS : MonoBehaviour
{
    public static GPS Instance { set; get; }

    public float latitude;
    public float longitude;
    public Text debug1;
    public Text debug2;
    public Text debug3;
    public float waitTime = 15.0f;

    private float timeAfterLastTrack = 0.0f;
    private bool sec = false;

    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(StartLocationService());
        //StartLocationService();

    }

    private IEnumerator StartLocationService()
    {
        sec = true;

        if(!Input.location.isEnabledByUser)
        {
            Debug.Log("User has not enabled GPS");
            yield break;
   
        }

        Input.location.Start();
        int maxWait = 20;
        while(Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.Log("Timmed out");
            yield break;
        }

        if(Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to deternal device location");
            yield break;
        }

        UpdateGPC();
        Input.location.Stop();

        sec = false;

        yield break;
    }

    private void Update()
    {
       if (timeAfterLastTrack < waitTime || sec)
        {
            timeAfterLastTrack += Time.deltaTime;
            debug3.text = timeAfterLastTrack.ToString(); //111111111111111------------------
        }
        else
        {
            StartCoroutine(StartLocationService());
        }
    }

    private void UpdateGPC()
    {
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        debug1.text = "latitude: " + latitude.ToString();
        debug2.text = "longitude: " + longitude.ToString();
        timeAfterLastTrack = 0.0f;
    }

    public void GetPositionButtonEvent()
    {
        StartCoroutine(StartLocationService());
    }
}
