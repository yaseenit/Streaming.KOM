## Configuration

The server can be configured by editing the file `configuration.yaml`. The following entries exist with their respective default values:
```yaml
server: dash # The server to run. One of dash, socket 
dash:
  host: 127.0.0.1 # IP to bind
  port: 5000 # Port to listen on
  media_path: "./media" # Folder, where the media files reside
```