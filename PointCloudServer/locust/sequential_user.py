from locust import HttpUser, task
import random
import urllib3

class SequentialUser(HttpUser):
    def on_start(self):
        self.frame=1
        self.client.get("/media/owlii_dancer_drc")
        self.client.verify = False
        urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)
    
    #@task
    def random_seek(self):
        self.client.get("/media/owlii_dancer_drc/16/{}.drc".format(str(random.randint(1,600)).zfill(8)))

    @task
    def sequential_frame(self):
        self.client.get("/media/owlii_dancer_drc/16/{}.drc".format(str(self.frame).zfill(8)))

        self.frame+=1
        if self.frame == 600:
            self.frame=1