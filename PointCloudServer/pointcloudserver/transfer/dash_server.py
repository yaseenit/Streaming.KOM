from fastapi import FastAPI, HTTPException
from fastapi.responses import Response
from fastapi.routing import APIRoute
import os
import time as T
import logging

import mimetypes
import asyncio
from hypercorn.config import Config
from hypercorn.asyncio import serve

class DASHServer():
    def __init__(self, host:str="127.0.0.1", port:int=5000, media_path:str="./media", cache=None, buffer_size:int=512):
        self.media_path=media_path

        self.config=Config()
        self.config.bind="{}:{}".format(host, port)

        self.app = FastAPI(
            routes=[
                APIRoute("/media/{name}", self.__media_mpd, methods=["GET"]),
                APIRoute("/media/{name}/{representation}/{segment}", self.__media_segment, methods=["GET"]),
            ]
        )

        mimetypes.add_type('pointcloud/ply', '.ply')
        mimetypes.add_type('pointcloud/drc', '.drc')
        mimetypes.add_type('pointcloud/mpeg-vpcc', '.vpcc')
        mimetypes.add_type('pointcloud/mpeg-gpcc', '.gpcc')
        mimetypes.add_type('application/zip', '.zip')

        self.cache = None
        if cache is not None:
            self.cache = cache


    async def __media_mpd(self, name):
        filename=os.path.join(self.media_path, name, "mpd.xml")

        data=self.load_file(filename)
        if data is None:
            raise HTTPException(status_code=404)

        return Response(content=data, media_type='application/dash+xml')

    async def __media_segment(self, name, representation, segment):
        filename=os.path.join(self.media_path, name, representation, segment)

        data = self.load_file(filename)
        if data is None:
            raise HTTPException(status_code=404)

        content_type=mimetypes.guess_type(filename, strict=False)[0]
        if content_type is None:
            raise HTTPException(status_code=406)

        return Response(content=data, media_type=content_type)

    def get_extension(self, path):
        return os.path.splitext(path)[1]

    def load_file(self, path):
        t_start=T.time()

        data=None
        if self.cache is not None:
            data=self.cache.get(path)
        if data is None:
            logging.debug("Cache miss")
            try:
                file=open(path, 'rb')
                data=file.read()
            except FileNotFoundError:
                data=None
            
            if self.cache is not None:
                self.cache.set(path, data)
        else:
            logging.debug("Cache hit")

        delta=T.time()-t_start
        logging.debug("Response Time \"{}\" {}ms".format(path, delta))
        return data

    def start(self):
        asyncio.run(serve(self.app, self.config))