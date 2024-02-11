import logging

from pointcloudserver.transfer.socket_server import SocketServer

def run(host: str, port: int) -> None:
    logging.info("Setting up socket server")
    server = SocketServer(
        host=host,
        port=port
    )
    
    logging.info("Starting socket server")
    server.start()
