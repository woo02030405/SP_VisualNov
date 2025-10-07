using UnityEngine;

public class SaveLoadBridge : MonoBehaviour
{
    public void OpenSaveScene()
    {
        SaveLoadPortal.Open(SaveLoadPortal.Mode.Save);
    }

    public void OpenLoadScene()
    {
        SaveLoadPortal.Open(SaveLoadPortal.Mode.Load);
    }
}
