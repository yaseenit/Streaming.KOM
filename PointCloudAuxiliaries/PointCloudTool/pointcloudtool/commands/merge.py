import argparse
import logging
import os
from typing import List

import pandas as pd

from pointcloudtool.io import PointCloudSerializerFactory
from pointcloudtool.operations import merge_pointclouds

def run(args: argparse.Namespace) -> None:
    """Run the render command with the given arguments.

    :param args: The arguments as a `Namespace`
    """
    input_directories: List[str] = args.inputDirectories
    output_directory: str = args.outputDirectory

    for file in os.listdir(input_directories[0]):
        input_directories_log = "\"" + "\", \"".join(input_directories) + "\""
        logging.info(f"Merging \"{file}\" from {input_directories_log} into \"{output_directory}\"")
        pointclouds: List[pd.DataFrame] = []
        for dir in input_directories:
            path = os.path.join(dir, file)
            pointcloud = PointCloudSerializerFactory().create_from_path(path).load(path)
            pointclouds.append(pointcloud)

        pointcloud = merge_pointclouds(pointclouds)
        path = os.path.join(output_directory, file)
        pointcloud = PointCloudSerializerFactory().create_from_path(path).save(pointcloud, path)