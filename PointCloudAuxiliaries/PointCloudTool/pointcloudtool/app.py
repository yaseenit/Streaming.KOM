#!/usr/bin/env python

import argparse
import logging
import logging.config

import pointcloudtool.commands.edit as edit
import pointcloudtool.commands.render as render
import pointcloudtool.commands.merge as merge

def setup_logging(verbose):
    """Setup logging

    :param verbose: Print debug messages if `True`, otherwise not
    """
    
    level = logging.DEBUG if verbose else logging.INFO
    
    formatter = logging.Formatter(fmt="%(asctime)s.%(msecs)03d %(thread)s %(levelname)s %(message)s", datefmt="%Y-%m-%dT%H:%M:%S")

    cli_handler = logging.StreamHandler()
    cli_handler.setFormatter(formatter)

    logger = logging.getLogger()
    logger.setLevel(level)
    logger.addHandler(cli_handler)

def main() -> None:
    ap = argparse.ArgumentParser(description="pointcloudtool - A tool to manipulate point clouds.")
    ap.add_argument("--verbose", default=False, action="store_true", help="Run the command with verbose logging enabled")
    
    sp = ap.add_subparsers(help="Command to run")

    sp_edit = sp.add_parser("edit", parents=[ap], add_help=False)
    sp_edit.add_argument("--inputPath", metavar="FILE", type=str, required=True, help="Path to an input file. Supports * as a wildcard.")
    sp_edit.add_argument("--outputPath", metavar="FILE", type=str, required=True, help="Path to an output file. Supports integer expansion by inserting e.g., %%04d for four zero padded digits.")
    sp_edit.add_argument("--outputPathOffset", metavar="INT", type=int, default=0, required=False, help="Indexing offset used, if integer expansion is applied in output path.")
    sp_edit.add_argument("--voxelSize", metavar="SIZE", type=float, default=0, required=False, help="If set, the point cloud will be voxelized with the given voxel size.")
    sp_edit.add_argument("--dedublicate", default=False, action="store_true", help="If set, dedubliate the point cloud before saving.")
    sp_edit.add_argument("--addAlphaChannel", default=False, action="store_true", help="If set, add an alpha channel with 255 as default to the point cloud value after loading.")
    sp_edit.add_argument("--dropAlphaChannel", default=False, action="store_true", help="If set, remove the alpha channel from the point cloud before saving.")
    sp_edit.add_argument("--dropNormals", default=False, action="store_true", help="If set, drop normal vectors from the point cloud before saving.")
    sp_edit.add_argument("--dryRun", default=False, action="store_true", help="Run the command without saving files to disk.")
    sp_edit.set_defaults(which="edit")

    sp_merge = sp.add_parser("merge", parents=[ap], add_help=False)
    sp_merge.add_argument('--inputDirectories', metavar='DIRS', type=str, nargs='+', required=True, help='A space separated list of directories. Point clouds with the same name will be merged.')
    sp_merge.add_argument('--outputDirectory', metavar='DIR', required=True, help='Directory where the merged files will be saved.')
    sp_merge.set_defaults(which="merge")

    sp_render = sp.add_parser("render", parents=[ap], add_help=False)
    sp_render.add_argument("--inputPath", metavar="FILE", type=str, required=True, help="Path to an input file.")
    sp_render.set_defaults(which="render")

    FLAGS=ap.parse_args()

    setup_logging(FLAGS.verbose)

    if FLAGS.which == "edit":
        edit.run(FLAGS)
    elif FLAGS.which == "merge":
        merge.run(FLAGS)
    elif FLAGS.which == "render":
        render.run(FLAGS)

if __name__ == "__main__":
    main()