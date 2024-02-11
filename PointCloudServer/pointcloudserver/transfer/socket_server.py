import logging
import socket
import time
import numpy as np
from scipy.spatial.transform import Rotation as R
import matplotlib.pyplot as plt

class SocketServer:
    def __init__(self, host='127.0.0.1', port=48002):
        self.SEND_FRAME=b'\x00'
        self.HOST = host
        self.PORT = port
        self.t_start=time.time()

    def get_random_sphere_volume(self, num_points):
        vec = np.random.randn(3, num_points)
        vec /= np.linalg.norm(vec, axis=0)
        return vec*1000

    def get_random_cube_surface(self, num_points):
        offset=np.array([-0.5,-0.5,-0.5])
        size=np.array([1,1,1])

        points=np.random.rand(num_points,3)
        points[:,0]*=size[0]
        points[:,1]*=size[1]
        points[:,2]*=size[2]

        while num_points>0:
            face=num_points%6
            if face==0:
                points[num_points-1,1]=0
            if face==1:
                points[num_points-1,1]=size[1]
            if face==2:
                points[num_points-1,2]=0
            if face==3:
                points[num_points-1,2]=size[2]
            if face==4:
                points[num_points-1,0]=0
            if face==5:
                points[num_points-1,0]=size[0]
            num_points-=1
        points+=offset
        return points*1000

    def get_random_sphere_surface(self, num_points):
        x = np.random.normal(size=(num_points, 3)) 
        x /= np.linalg.norm(x, axis=1)[:, np.newaxis]
        return x*1000

    def get_random_rgb_color(self, num_points):
        rgb=(np.random.rand(num_points,3)*255).astype('i')
        return rgb

    def rotate_pointcloud(self, pointcloud, degrees):
        rotation_radians = np.radians(degrees)
        rotation_axis = np.array([1, 1, 1])
        rotation_vector = rotation_radians * rotation_axis
        rotation = R.from_rotvec(rotation_vector)
        rotated_vec = rotation.apply(pointcloud)
        return rotated_vec

    def render(self, xyz, rgb=None):
        # Scale to [0,1] for matplotlib
        if rgb is not None:
           rgb/=255
        fig = plt.figure()
        ax = fig.add_subplot(projection='3d')
        ax.scatter(xyz[:,0], xyz[:,1], xyz[:,2], c=rgb)
        plt.show()
    
    def serialize(self, vertices, colors):
        # Convert number of points to 32 bit integer
        num_points = np.array([len(vertices)], dtype=np.int32)
        num_points = num_points.tobytes('C')

        # Convert vertices to 16 bit signed integer (signed short) array
        vertices = np.array(vertices, dtype=np.int16)
        vertices = vertices.tobytes('C')

        # Convert colors to 8 bit unsigned integer (byte) array
        colors = np.array(colors, dtype=np.uint8)
        colors = colors.tobytes('C')

        # Return concatenated byte array
        return num_points+vertices+colors

    def start(self):
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            s.bind((self.HOST, self.PORT))
            s.listen()
            logging.info("Listening on {}:{}".format(self.HOST, self.PORT))
            while True:
                conn, addr = s.accept()
                with conn:
                    logging.info('Accepted connection from {}'.format(addr))
                    while True:
                        data = conn.recv(1024)
                        if not data:
                            break
                        logging.debug("Received {} b from {}".format(len(data), addr))
                        logging.debug("Data received from {}: {}".format(addr, str(data)))

                        if data == self.SEND_FRAME:
                            logging.debug("Frame requested by {}".format(addr))
                            pointcloud=self.get_frame()
                            xyz = pointcloud[:,:3]
                            rgb = pointcloud[:,3:]
                            payload=self.serialize(xyz, rgb)
                            conn.sendall(payload)
                            logging.debug("Sent {} points ({} b)".format(len(pointcloud), len(payload)))

    def get_frame(self):
        num_points=10000
        ms_per_rot=10000

        xyz=self.rotate_pointcloud(
            self.get_random_cube_surface(num_points),
            (((time.time()-self.t_start)*1000)%ms_per_rot)*(360/ms_per_rot)
        )
        rgb=self.get_random_rgb_color(num_points)

        return np.hstack((xyz, rgb))