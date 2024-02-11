import numpy as np
import pandas as pd
from pandas.testing import assert_frame_equal
import pytest

import pointcloudtool.operations as ops
import pointcloudtool.io as io

pointcloud_xyz = pd.DataFrame.from_dict({
    "x": [0],
    "y": [1],
    "z": [2],
    },
    orient="columns"
)

pointcloud_xyz_dublicates = pd.DataFrame.from_dict({
    "x": [0, 0],
    "y": [1, 1],
    "z": [2, 2],
    },
    orient="columns"
)

pointcloud_xyz_xnynzn = pd.DataFrame.from_dict({
    "x": [0],
    "y": [1],
    "z": [2],
    "xn": [3],
    "yn": [4],
    "zn": [5],
    },
    orient="columns"
)

pointcloud_xyz_xn = pd.DataFrame.from_dict({
    "x": [0],
    "y": [1],
    "z": [2],
    "xn": [3],
    },
    orient="columns"
)

pointcloud_xyz_rgb = pd.DataFrame.from_dict({
        "x": [0],
        "y": [1],
        "z": [2],
        "r": [3],
        "g": [4],
        "b": [5],
    },
    orient="columns"
)

pointcloud_xyz_rgb_dublicates = pd.DataFrame.from_dict({
        "x": [0, 0],
        "y": [1, 1],
        "z": [2, 2],
        "r": [3, 6],
        "g": [4, 7],
        "b": [5, 8],
    },
    orient="columns"
)

pointcloud_xyz_rgba_default = pd.DataFrame.from_dict({
        "x": [0],
        "y": [1],
        "z": [2],
        "r": [3],
        "g": [4],
        "b": [5],
        "a": [255],
    },
    orient="columns"
)

pointcloud_xyz_rgba_custom = pd.DataFrame.from_dict({
        "x": [0],
        "y": [1],
        "z": [2],
        "r": [3],
        "g": [4],
        "b": [5],
        "a": [128],
    },
    orient="columns"
)

pointcloud_xyz_rgba_another_custom = pd.DataFrame.from_dict({
        "x": [6],
        "y": [7],
        "z": [8],
        "r": [9],
        "g": [10],
        "b": [11],
        "a": [129],
    },
    orient="columns"
)

pointcloud_xyz_rgba_merged = pd.DataFrame.from_dict({
        "x": [0, 6],
        "y": [1, 7],
        "z": [2, 8],
        "r": [3, 9],
        "g": [4, 10],
        "b": [5, 11],
        "a": [128, 129],
    },
    orient="columns"
)

pointcloud_xyz_rgb_xnynzn = pd.DataFrame.from_dict({
        "x": [0],
        "y": [1],
        "z": [2],
        "r": [3],
        "g": [4],
        "b": [5],
        "xn": [6],
        "yn": [7],
        "zn": [8],
    },
    orient="columns"
)

num_points = 1000
xyz_range = 2^16
pointcloud_xyz_rand = pd.DataFrame.from_dict({
    "x": np.random.rand(num_points) * xyz_range,
    "y": np.random.rand(num_points) * xyz_range,
    "z": np.random.rand(num_points) * xyz_range,
    },
    orient="columns"
)

def test_drop_normals_drops_normals():
    actual = ops.drop_normals(pointcloud_xyz_xnynzn.copy(deep=True))

    for normal in io.NORMALS:
        assert normal not in actual

def test_drop_normals_raise_no_exception_without_normals():
    pointcloud = pointcloud_xyz.copy(deep=True)
    ops.drop_normals(pointcloud)

def test_drop_normals_drops_partial_normals():
    pointcloud = pointcloud_xyz_xn.copy(deep=True)
    ops.drop_normals(pointcloud)

def test_drop_normals_drops_only_normals():
    actual = ops.drop_normals(pointcloud_xyz_rgb_xnynzn.copy(deep=True))
    expected = pointcloud_xyz_rgb

    assert_frame_equal(actual, expected)

def test_add_alpha_channel_reject_values_below_range():
    with pytest.raises(Exception):
        ops.add_alpha_channel(pointcloud_xyz_rgb.copy(deep=True), -1)

def test_add_alpha_channel_reject_value_above_range():
    with pytest.raises(Exception):
        ops.add_alpha_channel(pointcloud_xyz_rgb.copy(deep=True), 256)

def test_add_alpha_raise_exception_without_rgb():
    with pytest.raises(Exception):
        ops.add_alpha_channel(pointcloud_xyz.copy(deep=True))

def test_add_alpha_channel_adds_alpha():
    actual = ops.add_alpha_channel(pointcloud_xyz_rgb.copy(deep=True))
    expected = pointcloud_xyz_rgba_default

    assert_frame_equal(actual, expected)

def test_add_alpha_channel_raise_no_exception_when_alpha_exists():
    ops.add_alpha_channel(pointcloud_xyz_rgba_default.copy(deep=True))

def test_add_alpha_channel_does_not_change_alpha():
    actual = ops.add_alpha_channel(pointcloud_xyz_rgba_custom.copy(deep=True))
    expected = pointcloud_xyz_rgba_custom

    assert_frame_equal(actual, expected)

def test_drop_alpha_channel_drops_alpha():
    actual = ops.drop_alpha_channel(pointcloud_xyz_rgba_default.copy(deep=True))
    expected = pointcloud_xyz_rgb

    assert_frame_equal(actual, expected)

def test_drop_alpha_channel_raise_no_exception_when_alpha_not_exists():
    ops.drop_alpha_channel(pointcloud_xyz_rgb.copy(deep=True))

def test_dedublicate_pointcloud_dedublicates_pointcloud():
    actual = ops.dedublicate_pointcloud(pointcloud_xyz_dublicates.copy(deep=True))
    expected = pointcloud_xyz

    assert_frame_equal(actual, expected)

def test_dedublicate_pointcloud_keeps_other_columns():
    actual = ops.dedublicate_pointcloud(pointcloud_xyz_rgb_dublicates.copy(deep=True))
    expected = pointcloud_xyz_rgb

    assert_frame_equal(actual, expected)

def test_voxelize_pointcloud_result_is_smaller():
    voxelized = ops.voxelize_pointcloud(pointcloud_xyz_rand.copy(deep=True), 4)

    assert len(voxelized) < len(pointcloud_xyz_rand)

def test_voxelize_pointcloud_rejects_voxel_size_equal_zero():
    with pytest.raises(Exception):
        ops.voxelize_pointcloud(pointcloud_xyz_rand.copy(deep=True), 0)

def test_voxelize_pointcloud_rejects_voxel_size_smaller_than_zero():
    with pytest.raises(Exception):
        ops.voxelize_pointcloud(pointcloud_xyz_rand.copy(deep=True), -1)

def test_merge_pointclouds_does_merge():
    inputs = [
        pointcloud_xyz_rgba_custom.copy(deep=True),
        pointcloud_xyz_rgba_another_custom.copy(deep=True)
    ]
    actual = ops.merge_pointclouds(inputs)
    expected = pointcloud_xyz_rgba_merged

    assert_frame_equal(actual, expected)

def test_merge_pointclouds_rejects_unequal_properties():
    inputs = [
        pointcloud_xyz_rgba_custom.copy(deep=True),
        pointcloud_xyz_rgb.copy(deep=True)
    ]

    with pytest.raises(Exception):
        ops.merge_pointclouds(inputs)