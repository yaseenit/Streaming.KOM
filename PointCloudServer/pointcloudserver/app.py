#!/usr/bin/env python

import argparse
import logging
import logging.config

from pointcloudserver.persistence.configuration import Configuration
import pointcloudserver.commands.dash as dash
import pointcloudserver.commands.socket as socket
import pointcloudserver.commands.mpd as mpd

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
    ap_common = argparse.ArgumentParser(add_help=False)
    ap_common.add_argument("--verbose", default=False, action="store_true", help="Run the command with verbose logging enabled")
    ap_common.add_argument("--config", metavar="FILE", type=str, default=None, required=False, help="Path to a configuration file")
    ap_common.add_argument("--cache", metavar="CACHE", choices={"internal", "redis"}, default="internal", type=str, required=False, help="Cache backend to use")

    ap_main = argparse.ArgumentParser(description="pointcloudserver - A point cloud streaming server", add_help=True)
    sp = ap_main.add_subparsers(help="Commands")
    sp.required = True

    sp_edit = sp.add_parser("dash", parents=[ap_common], add_help=True)
    sp_edit.add_argument("--host", metavar="ADDRESS", default="127.0.0.1", type=str, required=False, help="Address to serve at")
    sp_edit.add_argument("--port", metavar="PORT", default=8080, type=int, required=False, help="Port to listen for incoming connections")
    sp_edit.add_argument("--mediaDir", metavar="DIR", type=str, required=True, help="Directory where media can be found")
    sp_edit.set_defaults(which="dash")

    sp_render = sp.add_parser("socket", parents=[ap_common], add_help=True)
    sp_render.add_argument("--host", metavar="ADDRESS", default="0.0.0.0", type=str, required=False, help="Address to serve at")
    sp_render.add_argument("--port", metavar="PORT", default=5000, type=int, required=False, help="Port to listen for incoming connections")
    sp_render.set_defaults(which="socket")

    sp_mpd = sp.add_parser("mpd", parents=[ap_common], add_help=True)
    sp_mpd.add_argument('--pretty', action='store_true', required=False, help='Print pretty formated xml instead of a single line')
    sp_mpd.add_argument('--outputFile', metavar='FILE', required=False, help='Save to a file instead of printing to command line')
    sp_mpd.add_argument('--framesPerSecond', metavar='FPS', required=False, help='Frames per second to calculate start and end time of periods')
    sp_mpd.add_argument('--baseUrl', metavar='URL', required=True, help='Base URL of the media on the webserver')
    sp_mpd.add_argument('mediaDir', metavar='FOLDER', nargs=1, help='Path to the media folder root')
    sp_mpd.set_defaults(which="mpd")

    args=ap_main.parse_args()

    setup_logging(args.verbose)

    config = Configuration()
    if args.config:
        config.load("configuration.yaml")
    config = config.config

    

    if args.which == "dash":
        dash.run(args.host, args.port, args.mediaDir, config)
    elif args.which == "socket":
        socket.run(args.host, args.port)
    elif args.which == "mpd":
        mpd.run(args)
if __name__ == "__main__":
    main()