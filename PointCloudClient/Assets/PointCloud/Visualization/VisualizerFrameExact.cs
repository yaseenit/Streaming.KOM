#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

using UnityEngine;
using Draco;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
#endif

public class VisualizerFrameExact : MonoBehaviour
{
    public GameObject PointCloud;
    public Camera Camera;
    public string InputDirectory = "";
    public string OutputFile = "";

    public int ObjectRate = 30;
    public int FirstFrame = 0;
    public int LastFrame = -1;
    public bool UseConfigurationFile = true;
    public string ConfigurationFile = "";
    private int frameRate = 30;
#if UNITY_EDITOR
    private RecorderController recorder = null;
#endif
    private int frameIndex=0;
    private Queue<RecordingConfig> configs = new Queue<RecordingConfig>();
    private int totalNumberOfConfigs = 0;
    private string[] sequenceFiles;
    private int recordingCooldown = 30;
    private int recordingCooldownCounter = 30;

    // Start is called before the first frame update
    void Start()
    {
        SimpleTimer.ShowDirectly = false;
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        this.recorder = new RecorderController(controllerSettings);

        if(UseConfigurationFile)
        {
            List<RecordingConfig> csvConfigs = loadConfiguration(ConfigurationFile);
            csvConfigs.ForEach(o => configs.Enqueue(o));
            this.totalNumberOfConfigs = csvConfigs.Count;
        }
        else
        {
            this.configs.Enqueue(new RecordingConfig(this.InputDirectory, this.OutputFile, this.Camera.transform.position, this.Camera.transform.eulerAngles, this.FirstFrame, this.LastFrame, this.ObjectRate));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(recordingCooldownCounter > 0)
        {
            recordingCooldownCounter--;
            return;
        }

        if(!recorder.IsRecording())
        {
            if(this.configs.Count == 0)
            {
                Debug.Log("Finished all recordings");
                UnityEditor.EditorApplication.isPlaying = false;
                return;
            }

            var config = this.configs.Dequeue();
            Debug.Log("Preparing sequence " + config.Name);
            prepare(config);

            Debug.Log("Starting recording");
            startRecording();
            return;
        }
        if(frameIndex < this.LastFrame)
        {
            Debug.Log($"Rendering Frame {this.frameIndex+1}/{this.LastFrame} of configuration {totalNumberOfConfigs-this.configs.Count}/{totalNumberOfConfigs}");
            RenderFrame();
            this.frameIndex++;
        }
        else
        {
            Debug.Log("Stopping recording");
            stopRecording();
            recordingCooldownCounter = recordingCooldown;
        }
    }

    private void RenderFrame()
    {
        Mesh mesh = loadMesh(sequenceFiles[getObjectIndex(frameIndex)]);
        updateMesh(mesh);
    }

    private int getObjectIndex(int frameIndex)
    {
        return frameIndex - (frameIndex % (this.frameRate/ (int)this.ObjectRate));
    }

    private void prepare(RecordingConfig config)
    {
        string path = config.Directory;

        Debug.Log("Analyzing sequence from "+path);
        string[] files;

        FileAttributes attr = File.GetAttributes(path);
        if (attr.HasFlag(FileAttributes.Directory))
            files = Directory.GetFiles(path);
        else
            files = new string[]{path};
        
        Array.Sort(files);

        Debug.Log("Found " + files.Length + " files");        
        sequenceFiles = files;

        this.InputDirectory = config.Directory;
        this.OutputFile = config.Name;
        this.frameIndex = config.FirstFrame;
        this.FirstFrame = config.FirstFrame;
        if(config.LastFrame == -1)
        {
            this.LastFrame = files.Length;
        }
        else
        {
            this.LastFrame = config.LastFrame;
        }
        this.ObjectRate = config.ObjectRate;
        prepareCamera(config.CameraPosition, config.CameraRotation);
        prepareRecorder(config.Name, 1080, 1080, config.FirstFrame, config.LastFrame, this.frameRate);
        RenderFrame();
    }

    private Mesh loadMesh(string path)
    {
        byte[] data = LoadFile(path);
        string extension = Path.GetExtension(path);
        Mesh mesh = null;

        if(extension == ".drc")
        {
            throw new NotImplementedException("Draco format currently not implemented for frame exact recording");
            //var draco = new DracoMeshLoader();
            //mesh = await draco.ConvertDracoMeshToUnity(data);
        }
        else if(extension == ".ply")
        {
            MemoryStream stream = new MemoryStream(data);
            stream.Position = 0;
            mesh = new PlyReader().ImportAsMesh(stream, path);
        }
        else
        {
            Debug.Log("Extension not found!");
        }
        //mesh.RecalculateBounds();
        return mesh;
    }

    private Dictionary<string, List<string>> readCSV(string path, char delimiter=';', bool header=true)
    {
        List<string>[] columns = new List<string>[1];
        List<string> headerRow = new List<string>();
        using(StreamReader reader = new StreamReader(@path))
        {
            int rowIndex = 0;
            while(!reader.EndOfStream)
            {
                string row = reader.ReadLine();
                string[] values = row.Split(delimiter);
                if(rowIndex == 0)
                {
                    columns = new List<string>[values.Length];
                    foreach(int columnIndex in Enumerable.Range(0, values.Length))
                    {
                        if(header)
                        {
                            headerRow.Add(values[columnIndex]);
                        }
                        else
                        {
                            headerRow.Add(columnIndex.ToString());
                        }

                        columns[columnIndex] = new List<string>();
                    }
                    if(header)
                    {
                        rowIndex++;
                        continue;                    
                    }
                }
                    
                foreach(int columnIndex in Enumerable.Range(0, values.Length))
                {
                    columns[columnIndex].Add(values[columnIndex]);
                }
                rowIndex++;                
            }

            Dictionary<string, List<string>> csv = new Dictionary<string, List<string>>();
            foreach(int columnIndex in Enumerable.Range(0, headerRow.Count))
            {
                csv[headerRow[columnIndex]] = columns[columnIndex];
            }

            return csv;
        }
    }

#if UNITY_EDITOR
    private List<RecordingConfig> loadConfiguration(string path)
    {
        var csv = readCSV(path);
        int rows = csv.ElementAt(0).Value.Count;
        int columns = csv.Count;

        List<RecordingConfig> configs = new List<RecordingConfig>();
        for(int row=0; row<rows; row++)
        {
            RecordingConfig config = new RecordingConfig(
                csv["directory"][row],
                csv["name"][row],
                new Vector3(
                    float.Parse(csv["cam_tx"][row]),
                    float.Parse(csv["cam_ty"][row]),
                    float.Parse(csv["cam_tz"][row])
                ),
                new Vector3(
                    float.Parse(csv["cam_rx"][row]),
                    float.Parse(csv["cam_ry"][row]),
                    float.Parse(csv["cam_rz"][row])
                ),
                int.Parse(csv["first_frame"][row]),
                int.Parse(csv["last_frame"][row]),
                int.Parse(csv["object_rate"][row])
            );
            configs.Add(config);
        }

        return configs;
    }
#endif
    private void updateMesh(Mesh mesh)
    {
        // Destroy old mesh
        Mesh current = PointCloud.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(current, true);

        // Set new mesh
        PointCloud.GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private byte[] LoadFile(string Path)
    {
        return File.ReadAllBytes(Path);
    }

#if UNITY_EDITOR
    private void prepareRecorder(string filename, int width, int height, int start, int end, int frameRate=30)
    {
        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var recorder = new RecorderController(controllerSettings);
        
        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        videoRecorder.name = filename;
        videoRecorder.Enabled = true;
        videoRecorder.VideoBitRateMode = VideoBitrateMode.High;
        
        videoRecorder.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = width,
            OutputHeight = height
        };
        
        videoRecorder.AudioInputSettings.PreserveAudio = false;
        videoRecorder.OutputFile = filename;
        
        controllerSettings.AddRecorderSettings(videoRecorder);
        controllerSettings.SetRecordModeToManual();
        //controllerSettings.SetRecordModeToFrameInterval(start, end);
        controllerSettings.FrameRate = frameRate;
        
        RecorderOptions.VerboseMode = false;
        recorder.PrepareRecording();
        
        this.recorder = recorder;
    }
#endif
    private void prepareCamera(Vector3 position, Vector3 rotation)
    {
        this.Camera.transform.position = position;
        this.Camera.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }

#if UNITY_EDITOR
    private void startRecording()
    {
        if( !recorder.IsRecording() )
        {
            recorder.StartRecording();
        }
    }
    private void stopRecording()
    {
        if( recorder.IsRecording() )
        {
            recorder.StopRecording();
        }
    }
#endif
}
#endif