from abc import ABC, abstractmethod
from argparse import ArgumentError
from collections import OrderedDict
import logging
import os

import DracoPy
import numpy as np
import open3d as o3d
import pandas as pd
from plyfile import PlyData, PlyElement

POINTS = ["x", "y", "z"]
NORMALS = ["xn", "yn", "zn"]
COLORS = ["r", "g", "b"]
ALPHA = ["a"]
REMISSION = ["remission"]

class Open3dConverter():
    def __init__(self) -> None:
        pass

    def convert_from(self, pointcloud: o3d.geometry.PointCloud) -> pd.DataFrame:
        """Convert the input pointcloud to a pandas `DataFrame`.

        :param pointcloud: An Open3D point cloud.
        :return: A `DataFrame` created from the input point cloud.
        """
        elements = OrderedDict()
        if pointcloud.has_points:
            points = np.asarray(pointcloud.points)
            elements["x"] = points[:, 0]
            elements["y"] = points[:, 1]
            elements["z"] = points[:, 2]

        if pointcloud.has_normals:
            normals = np.asarray(pointcloud.normals)
            if len(normals) != 0:
                elements["xn"] = normals[:, 0]
                elements["yn"] = normals[:, 1]
                elements["zn"] = normals[:, 2]

        # Open3D only loads rgb, no alpha channel
        if pointcloud.has_colors:
            colors = np.asarray(pointcloud.colors)
            if len(colors) != 0:
                elements["r"] = (colors[:, 0] * 255).round().astype(np.uint8)
                elements["g"] = (colors[:, 1] * 255).round().astype(np.uint8)
                elements["b"] = (colors[:, 2] * 255).round().astype(np.uint8)
        return pd.DataFrame.from_dict(elements, orient="columns")

    def convert_to(self, pointcloud: pd.DataFrame) -> o3d.geometry.PointCloud:
        """Convert the input `DataFrame` to a an Open3D point cloud.

        :param pointcloud: An `DataFrame` containing point cloud elements.
        :return: A Open3D point cloud created from the input `DataFrame`.
        """
        pointcloud_o3d = o3d.geometry.PointCloud()

        if set(POINTS).issubset(pointcloud.columns):
            points = pointcloud.filter(POINTS).to_numpy()
            pointcloud_o3d.points = o3d.utility.Vector3dVector(points)

        if set(COLORS).issubset(pointcloud.columns):
            colors = pointcloud.filter(COLORS).to_numpy()
            colors = colors.astype(np.float64) / 255.0
            pointcloud_o3d.colors = o3d.utility.Vector3dVector(colors)

        if set(NORMALS).issubset(pointcloud.columns):
            normals = pointcloud.filter(NORMALS).to_numpy()
            pointcloud_o3d.normals = o3d.utility.Vector3dVector(normals)

        return pointcloud_o3d

class PointCloudSerializer(ABC):
    @abstractmethod
    def __init__(self) -> None:
        raise NotImplementedError

    @abstractmethod
    def load(self, path: str) -> pd.DataFrame:
        raise NotImplementedError("Importing Draco format is not supported yet.")

    @abstractmethod
    def save(self, pointcloud: pd.DataFrame, path: str) -> None:
        raise NotImplementedError("Exporting to Draco format is not supported yet.")

class PlySerializer(PointCloudSerializer):
    def __init__(self) -> None:
        pass

    def load(self, path: str) -> pd.DataFrame:
        """Load a Ply formated point cloud from disk.

        :param path: Path to the file to load
        :return: The point loaded cloud
        """
        #pointcloud = PlyData.read(path)
        pointcloud = o3d.io.read_point_cloud(path) # Replace with PlyFile function for speed up
        return Open3dConverter().convert_from(pointcloud)

    def save(self, pointcloud: pd.DataFrame, path: str) -> None:
        """Save a point cloud as a Ply file

        :param pointcloud: Point cloud to save
        :param path: File path where to point cloud should be saved
        """
        elements = []
        types = []

        if set(POINTS).issubset(pointcloud.columns):
            elements.append(np.asarray(pointcloud.filter(POINTS)))
            types.extend((('x', 'single'), ('y', 'single'), ('z', 'single')))

        if set(NORMALS).issubset(pointcloud.columns):
            elements.append(np.asarray(pointcloud.filter(NORMALS)))
            types.extend((('nx', 'single'), ('ny', 'single'), ('nz', 'single')))

        if set(COLORS).issubset(pointcloud.columns):
            elements.append(np.asarray(pointcloud.filter(COLORS)))
            types.extend((('red', 'u1'), ('green', 'u1'), ('blue', 'u1')))

        if set(ALPHA).issubset(pointcloud.columns):
            elements.append(np.asarray(pointcloud.filter(ALPHA)))
            types.append(('alpha', 'u1'))
        elements = np.hstack(tuple(elements))

        vertices = np.array(
            [tuple(x) for x in elements.tolist()],# TODO: Slow! Should be replaced with something faster if possible
            dtype=types)
        
        PlyData([PlyElement.describe(vertices, 'vertex')], text=False).write(path)


class DracoSerializer(PointCloudSerializer):
    def __init__(self) -> None:
        pass

    def load(self, path: str) -> pd.DataFrame:
        """Load a Draco formated point cloud from disk.

        :param path: Path to the file to load
        :return: The point loaded cloud
        """
        with open(path, 'rb') as file:
            pointcloud = DracoPy.decode(file.read())

            elements = OrderedDict()
            if hasattr(pointcloud, "points"):
                points = pointcloud.points
                elements["x"] = points[:, 0]
                elements["y"] = points[:, 1]
                elements["z"] = points[:, 2]
            else:
                raise ArgumentError(f"Point cloud {path} contains no points.")

            if hasattr(pointcloud, "normals"):
                normals = pointcloud.normals
                elements["xn"] = normals[:, 0]
                elements["yn"] = normals[:, 1]
                elements["zn"] = normals[:, 2]
            return pd.DataFrame.from_dict(elements, orient="columns")
        
    def save(self, pointcloud: pd.DataFrame, path: str) -> None:
        """Save a point cloud as a Draco file

        :param pointcloud: Point cloud to save
        :param path: File path where to point cloud should be saved
        """
        points = pointcloud.filter(POINTS).to_numpy()

        data = DracoPy.encode(points, faces=None)
        with open(path, 'wb') as file:
            file.write(data)

class VelodyneSerializer(PointCloudSerializer):
    def __init__(self) -> None:
        pass

    def load(self, path: str) -> pd.DataFrame:
        """Load a Draco formated point cloud from disk.

        :param path: Path to the file to load
        :return: The point loaded cloud
        """
        scan = np.fromfile(path, dtype=np.float32).reshape((-1, 4))
        pointcloud = pd.DataFrame(scan, columns=["x", "y", "z", "remission"])

        return pointcloud

    def save(self, pointcloud: pd.DataFrame, path: str) -> None:
        """Save a point cloud as a Draco file

        :param pointcloud: Point cloud to save
        :param path: File path where to point cloud should be saved
        """
        xyz = pointcloud.filter(POINTS).to_numpy(dtype=np.float32)
        
        if set(REMISSION).issubset(pointcloud.columns):
            remission = pointcloud.filter(REMISSION)
        else:
            remission = np.ones(shape=(len(xyz), 1), dtype=np.float32)
        scan = np.hstack((xyz, remission))
        scan.tofile(path)        
  
class PointCloudSerializerFactory():
    def __init__(self) -> None:
        self.serializer = None

    def infer_format(self, path: str) -> str:
        """Try to infer the format of a file and return it.

        :param path: Path to the file to check
        :return: The infered format or `None` if it could not been infered
        """
        format = os.path.splitext(path)[1][1:]

        if format == "":
            raise ValueError(f"Format could not be infered from path {path}.")
        else:
            return format

    def create_from_path(self, path: str) -> PointCloudSerializer:
        format = self.infer_format(path)

        if format == "ply":
            return PlySerializer()
        elif format == "drc":
            return DracoSerializer()
        elif format == "bin":
            return VelodyneSerializer()
        else:
            raise ValueError(f"{format} format is not supported.")
            

def replace_extension(path: str, new_extension: str) -> str:
    """
    Remove a file extension from `path` and replace it `new_extension`.

    :param path: Path whose extension shall be replaced.
    :param new_extension: The new extension to apply.

    :return: Path with replaced extension.
    """
    return f"{os.path.splitext(path)[0]}.{new_extension}"