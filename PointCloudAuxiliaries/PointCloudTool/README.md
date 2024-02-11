# Point Cloud Tool

A tool to handle point clouds.


## Features

- [x] Voxelization
- [x] Normal vector removal
- [x] Simple point cloud visualization
- [x] Batch jobs
- [x] Alpha channel addition
- [x] Alpha channel removal
- [x] Point cloud dedublication
- [x] Draco transcoding support
- [ ] Normal vector recalculation

**Warning**: In the current implementation color decoding and color/normal encoding is not possible with Draco.

## Requirements

- Python >= 3.9


## Setup

Normal installation:
```bash
pip install .
```

Installation in editable mode with development dependencies:
```bash
pip install -e .[dev]
```

Removing the package:
```bash
pip uninstall pointcloudtool
```


## Tests

The underlying library for unit testing of this project is `pytest`. It will be installed along with the optional dev requirements.

Run all tests in the test directory:
```bash
python -m pytest test
```

Print additionally a statement coverage report to the terminal:
```bash
pytest --cov=pointcloudtool test
```
or create an detailed HTML report instead:
```bash
pytest --cov=pointcloudtool --cov-report=html test
```


## Usage

Show a general overview:
```bash
pointcloudtool --help
```

Show all options of a command (e.g., edit):
```bash
pointcloudtool edit --help
```

Voxelize a point cloud and show more logs:
```bash
pointcloudtool edit --inputPath /big.ply --outputPath /small.ply --voxelSize 5 --verbose
```

Add and an alpha channel and dedublicate the pointcloud:
```bash
pointcloudtool edit --inputPath /old.ply --outputPath /new.ply --addAlphaChannel --dedublicate
```

Remove the normal vectors from all point clouds from a directory and save them enumerated in another directory:
```bash
pointcloudtool edit --inputPath /path/to/sequence_*.ply --outputPath /other/path/sequence_nn_%04d.ply --dropNormals
```

Render a point cloud:
```bash
pointcloudtool render --inputPath /path/to/pointcloud.ply
```