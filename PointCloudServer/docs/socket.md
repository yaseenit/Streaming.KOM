## Socket Server
This is an implementation of the protocol used in [LiveScan3D-Hololens](https://github.com/MarekKowalski/LiveScan3D-Hololens) to stream pointclouds. A client can request a frame by sending `0x00` to the server over a TCP connection. The server responds with the pointcloud as a binary stream in the following format:

- Number of points as 32 bit signed integer
- Each point formated as xyz with a 16 bit signed integer per axis
- All colors as 8 bit unsigned integer each in the same order as the points