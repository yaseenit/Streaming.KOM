from typing import List

import open3d as o3d
import pandas as pd

from pointcloudtool import io as io

def add_alpha_channel(pointcloud: pd.DataFrame, alpha: int = 255) -> pd.DataFrame:
    """Add an alpha channel to the colors of the given point cloud.

    :param pointcloud: The point cloud to extend.
    :param alpha: Alpha value in the range of [0,255]. Defaults to 255.
    :return: The input point cloud with added alpha channel
    """

    if not set(io.COLORS).issubset(pointcloud.columns):
        raise ValueError("Adding an alpha channel requires existing RGB color values")

    if alpha < 0 or alpha > 255:
        raise ValueError(f"Alpha channel value must be in the range [0,255], but is {alpha}")

    if "a" not in pointcloud.columns:
        pointcloud["a"] = alpha

    return pointcloud

def drop_alpha_channel(pointcloud: pd.DataFrame) -> pd.DataFrame:
    """Remove the alpha channel of the given point cloud.

    :param pointcloud: The target point cloud
    :return: The input point cloud with removed alpha channel
    """
    if "a" in pointcloud.columns:
        pointcloud = pointcloud.drop("a", axis=1)
    return pointcloud

def drop_normals(pointcloud: pd.DataFrame) -> pd.DataFrame:
    """Remove the normal vectors of the given point cloud.

    :param pointcloud: The target point cloud
    :return: The input point cloud with removed normal vectors
    """
    for normal in io.NORMALS: 
        if normal not in pointcloud.columns:
            continue

        pointcloud = pointcloud.drop(normal, axis=1)

    return pointcloud
 
def dedublicate_pointcloud(pointcloud: pd.DataFrame) -> pd.DataFrame:
    """Remove points with dublicate coordinates from the given point cloud.

    :param pointcloud: The target point cloud
    :return: The input point cloud with removed dublicates
    """
    pointcloud.drop_duplicates(subset=io.POINTS, keep='first', inplace=True)
    return pointcloud

def voxelize_pointcloud(pointcloud: pd.DataFrame, voxel_size: int) -> pd.DataFrame:
    """Downsample the given point cloud by voxelization.

    :param pointcloud: The point cloud to voxelize
    :param voxel_size: The voxel size to apply
    :return: The downsampled point cloud
    """
    pointcloud_voxelized = o3d.geometry.PointCloud.voxel_down_sample(io.Open3dConverter().convert_to(pointcloud), voxel_size)

    return io.Open3dConverter().convert_from(pointcloud_voxelized)

def merge_pointclouds(pointclouds: List[pd.DataFrame]) -> pd.DataFrame:
    """
    Merge a list of point clouds into a single point cloud.

    :param pointclouds: A list of point clouds to merge.

    :return: The merged point cloud.
    """
    if not all([set(pointclouds[0].columns) == set(pointcloud.columns) for pointcloud in pointclouds]):
        raise ValueError("Merging requires point clouds to feature the same properties.")

    pointcloud = pd.concat(pointclouds, ignore_index=True)
    return pointcloud