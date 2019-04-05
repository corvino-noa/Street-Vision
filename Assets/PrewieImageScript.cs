using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrewieImageScript : MonoBehaviour
{
    private Image prewieImage;
    private static readonly float minScale = 0.05f;
    private bool scaleSecurity;
    private Vector3 startScale;
    public float speedOfScaling;



    private void Start()
    {
        prewieImage = gameObject.GetComponent<Image>();
        scaleSecurity = false;
        startScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
    }

    // Update is called once per frame
    private void Update()
    {
        if (scaleSecurity)
        {
            Vector2 scaleNow = new Vector2(gameObject.transform.localScale.x, gameObject.transform.localScale.y);
            scaleNow = Vector2.MoveTowards(scaleNow, new Vector2(minScale, minScale), Time.deltaTime * speedOfScaling);
            if (scaleNow.x <= minScale)
            {
                scaleSecurity = false;
                prewieImage.enabled = false;
                gameObject.transform.localScale = startScale;
            }
            else
            {
                gameObject.transform.localScale = new Vector3 (scaleNow.x, scaleNow.y,gameObject.transform.localScale.z);
            }

        }
        
    }

    public void startAnimation(Texture2D texture)
    {
        prewieImage.sprite = Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        prewieImage.enabled = true;
        scaleSecurity = true;
    }
}
