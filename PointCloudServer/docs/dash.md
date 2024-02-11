## DASH Server
Serves media according to [ISO/IEC 23009-1](https://www.iso.org/standard/75485.html) under the address `http://127.0.0.1:5000/media`. The following folder structure is used to fetch media: Inside the `./media` (default) folder each streamable media has an own folder - for this example named `foo`. This folder must contain a MPD file named `mpd.xml`, which saves the streams meta data. The file can be downloaded by clients from the address `http://127.0.0.1/media/foo`.

Additionally subfolders can be placed in the media folders root, e.g. named `bar`. Files from subfolders will be served at `http://127.0.0.1/media/foo/bar/FILENAME`.

This example could lead to the following structure:
```
data
  foo
    mpd.xml
    bar
	  0.ply
	  1.ply
	  2.ply
```

The command `pointcloudserver mpd` can be used to generate a basic MPD file from a media folder. A usage guide will show up by calling `pointcloudserver mpd --help`.