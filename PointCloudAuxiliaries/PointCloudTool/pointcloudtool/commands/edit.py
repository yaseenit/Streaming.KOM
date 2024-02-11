import argparse
import logging
import glob
import os
import re
import pandas as pd

from pointcloudtool.io import PointCloudSerializerFactory
import pointcloudtool.operations as ops

def run(args: argparse.Namespace) -> None:
    """Run the edit command with the given arguments.

    :param args: Parameters as a `Namespace`
    """

    files = glob.glob(args.inputPath)
    files = sorted(files)
    
    for file, i in zip(files, range(len(files))):
        logging.debug(f"Loading file {file}")
        pointcloud = PointCloudSerializerFactory().create_from_path(file).load(file)

        pointcloud = pipeline(pointcloud, args)
       
        outputPath=args.outputPath
        if os.path.isdir(args.outputPath):
            outputPath = os.path.join(args.outputPath, os.path.basename(file))
        
        if re.compile(r"%[0-9]+d").search(outputPath):
            outputPath = outputPath % (i+args.outputPathOffset)
        

        logging.debug(f"Saving point cloud to {outputPath}")
        if not args.dryRun:
            pointcloud = PointCloudSerializerFactory().create_from_path(outputPath).save(pointcloud, outputPath)

    logging.debug(f"Finished processing")

def pipeline(pointcloud: pd.DataFrame, args) -> pd.DataFrame:
    """Run the edit pipeline on the given point cloud and args.

    :param pointcloud: Point cloud to edit
    :param args: Arguments to apply
    :return: The edited point cloud
    """
    if args.voxelSize > 0:
        logging.debug(f"Applying voxelization with voxel size {args.voxelSize}")
        pointcloud = ops.voxelize_pointcloud(pointcloud, args.voxelSize)

    if args.addAlphaChannel:
        alpha=255
        logging.debug(f"Adding alpha channel with value {alpha}")
        pointcloud = ops.add_alpha_channel(pointcloud, alpha=alpha)

    if args.dropNormals:
        logging.debug(f"Dropping normal vectors")
        pointcloud = ops.drop_normals(pointcloud)
    
    if args.dedublicate:
        logging.debug(f"Dedublicating point cloud")
        pointcloud = ops.dedublicate_pointcloud(pointcloud)

    if args.dropAlphaChannel:
        logging.debug(f"Removing alpha channel from point cloud")
        pointcloud = ops.drop_alpha_channel(pointcloud)
    
    return pointcloud