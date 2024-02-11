import argparse
import logging
import os

from pointcloudtool.visualization import render_pointcloud
from pointcloudtool.io import PointCloudSerializer, PointCloudSerializerFactory

def run(args: argparse.Namespace) -> None:
    """Run the render command with the given arguments.

    :param args: The arguments as a `Namespace`
    """
    if not os.path.isfile(args.inputPath):
        logging.error("inputPath must point to a file")
        quit()

    logging.debug(f"Opening {os.path.basename(args.inputPath)}")
    pointcloud = PointCloudSerializerFactory().create_from_path(args.inputPath).load(args.inputPath)
    render_pointcloud(pointcloud, window_name=args.inputPath, point_size=1.0)