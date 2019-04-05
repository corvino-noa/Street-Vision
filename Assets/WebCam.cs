using UnityEngine;
using System.Collections;
using System.IO;

public class WebCam : MonoBehaviour
{

    WebCamTexture webCamTexture;

    void Start()
    {
        webCamTexture = new WebCamTexture();
        gameObject.GetComponent<Renderer>().material.mainTexture = webCamTexture;
        webCamTexture.Play();
    }

    public void TakePhoto()
    {

        // NOTE - you almost certainly have to do this here:

        //yield return new WaitForEndOfFrame();

        // it's a rare case where the Unity doco is pretty clear,
        // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
        // be sure to scroll down to the SECOND long example on that doco page 

        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        //Encode to a PNG
        byte[] bytes = photo.EncodeToPNG();
        //Write out the PNG. Of course you have to substitute your_path for something sensible
        File.WriteAllBytes(Application.persistentDataPath + "/YUV2RGB.png", bytes);
    }
}
