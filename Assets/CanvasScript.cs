using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    [SerializeField]
    private Image selectPaintObject;
    [SerializeField]
    private GameObject logPanel;

    private string path = "Prefabs/1";

    public void SelectPaintObjectEvent(string name)
    {
        Sprite fromPrefab = Resources.Load<Sprite>("PaintObjectImages/" +name);

        selectPaintObject.sprite = fromPrefab;

        path = "Prefabs/" + name;
    }

    public string GetPathToPrefub()
    {
        return path;
    }

    public void SetVisibleLogEvent()
    {
        logPanel.SetActive(!logPanel.activeSelf);
    }
}
