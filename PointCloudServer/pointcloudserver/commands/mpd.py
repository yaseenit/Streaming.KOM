import xml.etree.ElementTree as ET
from argparse import Namespace

from pointcloudserver.dash.mpd import build_multiple_object_mpd, build_single_object_mpd, get_depth, save_xml, serialize_xml

def run(args: Namespace):
    media_dir=args.mediaDir[0]
    pretty=args.pretty
    base_url=args.baseUrl
    output_file=args.outputFile
    fps=None
    if args.framesPerSecond is not None:
        fps=int(args.framesPerSecond)
    mpd_type="static"

    mpd = ET.Element("MPD")
    mpd.set("type", mpd_type)
    mpd.set("minBufferTime", str(1.0/float(fps)))

    mpd_url = ET.SubElement(mpd, "BaseURL")
    mpd_url.text = base_url

    depth = get_depth(media_dir)
    if depth == 2:
        mpd = build_single_object_mpd(mpd, media_dir, fps)
    elif depth == 3:
        mpd = build_multiple_object_mpd(mpd, media_dir, fps)
    mediaPresentationDuration = len(mpd.findall("Period")) * (1/fps)
    mpd.set("mediaPresentationDuration", str(mediaPresentationDuration))
    if output_file is not None:
        save_xml(mpd, output_file, pretty=pretty)
    else:
        print(serialize_xml(mpd, pretty=pretty))
