import xml.etree.ElementTree as ET
from xml.dom import minidom
import os
import uuid

def save_xml(root, path, pretty=False):
    with open(path, "w") as f:
        f.write(serialize_xml(root, pretty=pretty))

def serialize_xml(root, pretty=False):
    if pretty:
        return minidom.parseString(ET.tostring(root)).toprettyxml(indent="  ")
    else:
        return ET.tostring(root, encoding='unicode', method='xml')

def get_id(root, tag, id):
    for child in root:
        if child.tag is tag:
            if child.attrib["id"] == str(id):
                return child
    return None

def get_depth(path, depth=0):
    if not os.path.isdir(path): return depth
    max_depth=depth
    for entry in os.listdir(path):
        full_path = os.path.join(path, entry)
        max_depth = max(max_depth, get_depth(full_path, depth+1))
    return max_depth

def infer_mime_type(path: str) -> str:
    """Infer and return the MIME-Type of the given file

    :param path: The path to analyse
    :return: The infered MIME-Type on success, else `None`
    """
    # TODO: Replace everything with mimetypes package or dictionary
    left, right = os.path.splitext(path)

    if right == ".drc":
        return "pointcloud/drc"

    if right == ".gpcc":
        return "pointcloud/gpcc"

    if right == ".mp4":
        return "video/mp4"

    if right == ".ply":
        return "pointcloud/ply"

    if right == ".vpcc":
        return "pointcloud/vpcc"

    if right == ".zip":
        _, right = os.path.splitext(left)
        if right == ".ply":
            return "pointcloud/ply+zip"
        if right == ".drc":
            return "pointcloud/drc+zip"


    return None
                
def build_single_object_mpd(mpd, path, fps):
    entries=os.listdir(path)
    for entry in entries:
        entry_path=os.path.join(path, entry)
        if os.path.isdir(entry_path):
            sub_entries=os.listdir(entry_path)
            period_counter=0
            for sub_entry in sub_entries:
                sub_entry_path=os.path.join(entry_path, sub_entry)
                period=get_id(mpd, "Period", period_counter)
                adapation_set=None
                if period is None:
                    period=ET.SubElement(mpd, "Period")
                    period.set("id", str(period_counter))
                    if fps is not None:
                        frame_time=1/fps
                        start=period_counter/fps
                        end=start+frame_time
                        period.set("start", "PT"+str(start)+"S")
                        period.set("end", "PT"+str(end)+"S")
                        period.set("duration", "PT"+str(frame_time)+"S")

                    adapation_set=ET.SubElement(period, "AdaptationSet")
                    mime_type = infer_mime_type(sub_entry_path)
                    if mime_type is not None:
                        adapation_set.set("mimeType",mime_type)
                    adapation_set.set("id", str(period_counter))
                else:
                    adapation_set=period.find("AdaptationSet")

                id        = str(uuid.uuid4())
                size      = os.path.getsize(os.path.join(path, entry, sub_entry))
                bandwidth = size * int(fps)

                representation=ET.SubElement(adapation_set, "Representation")
                representation.set("id", str(id))
                representation.set("size", str(size))
                representation.set("bandwidth", str(bandwidth))

                representation_url=ET.SubElement(representation, "BaseURL")
                representation_url.text=entry+"/"+sub_entry
                period_counter+=1
    return mpd

def build_multiple_object_mpd(mpd, path, fps):
    objects=os.listdir(path)
    for object in objects:
        object_path=os.path.join(path, object)
        if os.path.isdir(object_path):
            qualtities=os.listdir(object_path)
            for quality in qualtities:
                quality_path=os.path.join(object_path, quality)
                sequence=os.listdir(quality_path)
                period_counter=0
                for frame in sequence:
                    frame_path=os.path.join(quality_path, frame)
                    period=get_id(mpd, "Period", period_counter)
                    if period is None:
                        period=ET.SubElement(mpd, "Period")
                        period.set("id", str(period_counter))
                        if fps is not None:
                            frame_time=1/fps
                            start=period_counter/fps
                            end=start+frame_time
                            period.set("start", "PT"+str(start)+"S")
                            period.set("end", "PT"+str(end)+"S")
                            period.set("duration", "PT"+str(frame_time)+"S")

                    adapation_set=get_id(period, "AdaptationSet", object)
                    if adapation_set is None:
                        adapation_set=ET.SubElement(period, "AdaptationSet")
                        adapation_set.set("id", object)
                        mime_type = infer_mime_type(frame_path)
                        if mime_type is not None:
                            adapation_set.set("mimeType", mime_type)

                    id        = str(uuid.uuid4())
                    size      = os.path.getsize(os.path.join(path, object, quality, frame))
                    bandwidth = int(size * fps)
    
                    representation=ET.SubElement(adapation_set, "Representation")
                    representation.set("id", id)
                    representation.set("size", size)
                    representation.set("bandwidth", bandwidth)

                    representation_url=ET.SubElement(representation, "BaseURL")
                    representation_url.text=object+"/"+quality+"/"+frame
                    period_counter+=1
    return mpd
