import logging

import redis

from pointcloudserver.transfer.buffer import Buffer
from pointcloudserver.transfer.dash_server import DASHServer

def run(host: str, port: int, media_path: str, config: dict) -> None:
    cache = None
    if config['cache']['use'] == 'internal':
        logging.info(f"Setting up internal cache") 
        cache = Buffer(logger=logging.getLogger('root'), max_size=config['cache']['internal']['buffer_size'])
    elif config['cache']['use'] == 'redis':
        logging.info(f"Setting up redis cache") 
        cache = redis.Redis(host=config['cache']['redis']['host'], port=config['cache']['redis']['port'], db=0)
    else:
        logging.info(f"Setting up no cache") 

    logging.info("Setting up DASH server")
    server=DASHServer(
        host=host,
        port=port,
        media_path=media_path,
        cache=cache
    ) 

    logging.info("Starting DASH server")
    server.start()