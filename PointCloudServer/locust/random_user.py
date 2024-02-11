from locust import HttpUser, task
import random

class SequentialUser(HttpUser):
    def on_start(self):
        pass
    
    @task
    def random_seek(self):
        self.client.get("/media/owlii_dancer_drc/16/{}.drc".format(str(random.randint(1,600)).zfill(8)))