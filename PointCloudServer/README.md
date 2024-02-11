# Point Cloud Server
A simple Point Cloud Streaming Server written in Python.


## Installation

This project supports Python versions <= 3.9. To install this package open a terminal in the projects root directory and call
```bash
pip install .
```
for a user installation. Alternatively the project can be installed in editable mode with development dependencies added:

```bash
pip install -e .[dev]
```


## Testing
Calling `pytest .` from the root directory will run all tests from the `tests` folder. Can only be used in conjunction with the development environment.


## Usage

Run a DASH server:
```bash
pointcloudserver dash --config /path/to/config.yaml
```

Run a socket server listening on localhost and port 5000:
```bash
pointcloudserver socket --host 127.0.0.1 --port 5000
```


## Traffic Shaping

If build with the included Dockerfile and/or started with the docker-compose file, traffic shaping can be applied to the server. For this to work, Linux has to be used as the host system for docker.

After starting the container, connect to it with `docker exec -it CONTAINER_NAME /bin/bash`. Now the full pallet of [tcconfig](https://github.com/thombashi/tcconfig) can be used. Here are a few selected examples from there:

- Limit the bandwidth: `tcset eth0 --rate 100Kbps`
- Add latency: `tcset eth0 --delay 100ms`
- Drop packages: `tcset eth0 --loss 0.1%`
- Add multiple rules at once: `tcset eth0 --rate 100Kbps --delay 100ms --loss 0.1%`
- Remove all rules: `tcdel eth0 --all`

The session can be left at any time by calling `exit`.