import logging
from typing import OrderedDict

class Buffer:
    def __init__(self, logger=None, max_size=512):
        self.max_size=max_size*1024*1024 # in bytes

        if logger is None:
            self.logger = logging.getLogger("root")
        else:
            self.logger = logger
        
        self.init_buffer()
        self.logger.info("Initialized buffer with {} Mb size limit".format(max_size))
    
    def init_buffer(self):
        self.buffer=OrderedDict()

    def set(self, key, value):
        if key is None:
            return

        if value is None:
            return

        need=len(value)
        if need > self.get_max_size():
            self.logger.error("Size of value for key \"{}\" is to large ({})".format(key, need))
            return

        missing=need-self.get_free_size()
        if key in self.buffer:
            self.logger.debug("Updating key \"{}\"".format(key))
            missing-=len(self.buffer[key])
            self.delete(key)
        else:
            self.logger.debug("Adding key \"{}\"".format(key))

        if missing > 0:
            self.logger.debug("{} b missing in buffer".format(missing))
            self.clean(missing)

        self.buffer[key]=value
    
    def get(self, key):
        if key not in self.buffer:
            self.logger.debug("Value for key \"{}\" not found".format(key))
        else:
            self.logger.debug("Found value for key \"{}\"".format(key))
            return self.buffer[key]

    def delete(self, key):
        if key in self.buffer:
            self.logger.debug("Removing key \"{}\"".format(key))
            del self.buffer[key]

    def clean(self, size=0):
        if size < 0:
            return

        if size == 0 or size > self.get_current_size():
            self.init_buffer()
            self.logger("Cleaned buffer completly")
            return

        cleaned=0
        while cleaned < size:
            k = next(iter(self.buffer))
            cleaned+=len(self.buffer[k])
            self.delete(k)
        self.logger.debug("Cleaned {} b".format(cleaned))
    
    def get_max_size(self):
        return self.max_size

    def get_current_size(self):
        size=0
        for k,v in self.buffer.items():
            size+=len(v)
        
        return size

    def get_free_size(self):
        return self.get_max_size() - self.get_current_size()
