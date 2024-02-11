# Point Cloud Client

## Project Setup

1. Download and install Unity Hub
2. Install Unity Version 2020.3.11f1 with UWP and, if not already installed, Visual Studio 2019 selected 
3. Download and add this project to Unity Hub and start it
4. Open the scene `Assets/Scenes/DASHDemo`
5. Point the `Media URL` of the `Point_Cloud` object to a DASH endpoint or a local directory
6. Run the scene by pressing the "Play" button

A program servering a DASH endpoint and setup instructions for it can be found in the project `PointCloudServer`. The following point cloud formats are supported by the client:
- PLY
- Draco (only with rgba colors!)

## Sources

The original shader files used by this project can be found at [https://github.com/SFraissTU/BA_PointCloud](https://github.com/SFraissTU/BA_PointCloud)