import numpy as np
from pointcloudserver.transfer.socket_server import SocketServer

vertices=np.array([
    [0,1,2],
    [3,4,5]
])
colors=np.array([
    [6,7,8],
    [9,10,11]
])

# Expected format:
# - Number of vertices as integer (32 bit)
# - x,y,z coordinates of vertices as short (16 bit)
# - Colors as bytes (8 bit)
serialized_expected=b'\x02\x00\x00\x00\x00\x00\x01\x00\x02\x00\x03\x00\x04\x00\x05\x00\x06\x07\x08\x09\x0a\x0b'

class TestSerialize:
    def test_serialize_to_correct_format(self):
        server=SocketServer()
        serialized_actual=server.serialize(vertices, colors)
        assert serialized_actual==serialized_expected
