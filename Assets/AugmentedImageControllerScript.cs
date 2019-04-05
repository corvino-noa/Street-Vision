using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using System;

public class AugmentedImageControllerScript : MonoBehaviour
{
    [SerializeField]
    private AugmentedImageVisualizerScript augmentedImageVisualizer;

    private readonly Dictionary<int, AugmentedImageVisualizerScript> visualizers =
        new Dictionary<int, AugmentedImageVisualizerScript>();

    private readonly List<AugmentedImage> images = new List<AugmentedImage>();

    private void Update()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        Session.GetTrackables(images, TrackableQueryFilter.Updated);
        VisualizeTrackables();
    }

    private void VisualizeTrackables()
    {
        foreach (var image in images)
        {
            var visualizer = GetVisualizer(image);

            if (image.TrackingState == TrackingState.Tracking && visualizer == null)
            {
                AddVisualizer(image);
            }
            else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
            {
                RemoveVisualizer(image, visualizer);
            }
        }
        //21:30 https://www.youtube.com/watch?v=GkzMFNmvums
    }

    private void RemoveVisualizer(AugmentedImage image, AugmentedImageVisualizerScript visualizer)
    {
        visualizers.Remove(image.DatabaseIndex);
        Destroy(visualizer.gameObject);
    }

    private void AddVisualizer(AugmentedImage image)
    {
        var anchor = image.CreateAnchor(image.CenterPose);
        var visualizer = Instantiate(augmentedImageVisualizer, anchor.transform);
        visualizer.image = image;
        visualizers.Add(image.DatabaseIndex, visualizer);
    }

    private AugmentedImageVisualizerScript GetVisualizer(AugmentedImage image)
    {
        AugmentedImageVisualizerScript visualizer;
        visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
        return visualizer;
    }
}
