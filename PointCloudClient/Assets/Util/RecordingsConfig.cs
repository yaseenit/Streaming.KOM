using UnityEngine;

public class RecordingConfig
{
    public string Name;
    public string Directory;
    public int ObjectRate;
    public int FirstFrame;
    public int LastFrame;
    public Vector3 CameraPosition;
    public Vector3 CameraRotation;

    public RecordingConfig(string directory, string name, Vector3 cameraPosition, Vector3 cameraRotation, int firstFrame=0, int lastFrame=0, int objectRate=30)
    {
        this.Directory = directory;
        this.Name = name;
        this.ObjectRate = objectRate;
        this.FirstFrame = firstFrame;
        this.LastFrame = lastFrame;

        this.CameraPosition = cameraPosition;
        this.CameraRotation = cameraRotation;
    }
}