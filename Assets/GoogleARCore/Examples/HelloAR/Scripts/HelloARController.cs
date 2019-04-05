//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.HelloAR
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using GoogleARCore.Examples.ComputerVision;
    using UnityEngine;
    using UnityEngine.UI;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class HelloARController : MonoBehaviour
    {
        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject DetectedPlanePrefab;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        //public GameObject AndyPlanePrefab;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a feature point.
        /// </summary>
        //public GameObject AndyPointPrefab;

        /// <summary>
        /// The rotation in degrees need to apply to model when the Andy model is placed.
        /// </summary>
        private const float k_ModelRotation = 180.0f;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;

        public CanvasScript canvasS;
        public Text debugT;
        public PrewieImageScript prewieImageScript;


        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            _UpdateApplicationLifecycle();

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began) //клик по экрану 
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;
            //TODO: add image saving from https://stackoverflow.com/questions/49579334/save-acquirecameraimagebytes-from-unity-arcore-to-storage-as-an-image
            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Choose the Andy model for the Trackable that got hit.
                    //GameObject prefab;
                    //if (hit.Trackable is FeaturePoint)
                    //{
                    //    prefab = AndyPointPrefab;
                    //}
                    //else
                    //{
                    //    prefab = AndyPlanePrefab;
                    //}

                    GameObject prefab = Resources.Load<GameObject>(canvasS.GetPathToPrefub());


                    // Instantiate Andy model at the hit pose.
                    var andyObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                    andyObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make Andy model a child of the anchor.
                    andyObject.transform.parent = anchor.transform;


                    //TakeImage();
                    using (var image = Frame.CameraImage.AcquireCameraImageBytes())
                    {
                        if (!image.IsAvailable)
                        {
                            return;
                        }

                        _OnImageAvailable(image.Width, image.Height, image.YRowStride, image.Y, 0);
                    }
                }
            }
        }




        private void _OnImageAvailable(int width, int height, int rowStride, IntPtr pixelBuffer, int bufferSize)
        {

            Texture2D m_TextureRender = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            byte[] bufferYUV = new byte[width * height * 3 / 2];
            bufferSize = width * height * 3 / 2;
            System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, bufferYUV, 0, bufferSize);
            Color color = new Color();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    float Yvalue = bufferYUV[y * width + x];
                    float Uvalue = bufferYUV[(y / 2) * (width / 2) + x / 2 + (width * height)];
                    float Vvalue = bufferYUV[(y / 2) * (width / 2) + x / 2 + (width * height) + (width * height) / 4];
                    color.r = Yvalue + (float)(1.37705 * (Vvalue - 128.0f));
                    color.g = Yvalue - (float)(0.698001 * (Vvalue - 128.0f)) - (float)(0.337633 * (Uvalue - 128.0f));
                    color.b = Yvalue + (float)(1.732446 * (Uvalue - 128.0f));

                    color.r /= 255.0f;
                    color.g /= 255.0f;
                    color.b /= 255.0f;

                    if (color.r < 0.0f)
                        color.r = 0.0f;
                    if (color.g < 0.0f)
                        color.g = 0.0f;
                    if (color.b < 0.0f)
                        color.b = 0.0f;

                    if (color.r > 1.0f)
                        color.r = 1.0f;
                    if (color.g > 1.0f)
                        color.g = 1.0f;
                    if (color.b > 1.0f)
                        color.b = 1.0f;

                    color.a = 1.0f;
                    m_TextureRender.SetPixel(width - 1 - x, height-1-y, color);
                }
            }

            try
            {
                m_TextureRender.Apply();
                var encodedpng = m_TextureRender.EncodeToPNG();
                var path = Application.persistentDataPath;
                string time = System.DateTime.UtcNow.ToString();
                //File.WriteAllBytes(path + "/"+time+".png", encodedpng);
                File.WriteAllBytes(path + "/YUV2RGB.png", encodedpng);
                debugT.text = path + "/YUV2RGB.png";
                prewieImageScript.startAnimation(m_TextureRender);
            }
            catch (Exception ex)
            {
                debugT.text = ex.Message;
            }

        }


















        //////////private TextureReader m_CachedTextureReader = new TextureReader();

        //////////public void TakeImage()
        //////////{
        //////////    m_CachedTextureReader = new TextureReader();
        //////////    // Function to call from a button to take a picture with the TextureReaderApi
        //////////    m_CachedTextureReader.enabled = true;
        //////////    m_CachedTextureReader.OnImageAvailableCallback += _OnImageAvailable;
        //////////}

        //////////private Texture2D m_EdgeDetectionBackgroundTexture = null;
        //////////private byte[] m_EdgeDetectionResultImage = null;


        ////private void _OnImageAvailable(TextureReaderApi.ImageFormatType format, int width, int height, IntPtr pixelBuffer, int bufferSize)
        ////{
        ////    m_CachedTextureReader.enabled = false;
        ////    m_CachedTextureReader.OnImageAvailableCallback -= _OnImageAvailable;

        ////    m_EdgeDetectionBackgroundTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
        ////    m_EdgeDetectionResultImage = new byte[width * height * 4];

        ////    System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, m_EdgeDetectionResultImage, 0, bufferSize);

        ////    // Save Image
        ////    string date = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        ////    m_EdgeDetectionBackgroundTexture.LoadRawTextureData(m_EdgeDetectionResultImage);
        ////    m_EdgeDetectionBackgroundTexture.Apply();

        ////    var encodedPng = m_EdgeDetectionBackgroundTexture.EncodeToPNG();
        ////    var path = Application.persistentDataPath;
        ////    File.WriteAllBytes(path + "/images/" + date + ".png", encodedPng);
        ////    Debug.Log(path + "/images/" + date + ".png");
        ////}

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
