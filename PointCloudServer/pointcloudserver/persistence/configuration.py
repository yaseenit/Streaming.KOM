import yaml

class Configuration():
    def __init__(self, path:str=None):
        """
        Initialize the configuration with default values and loads
        optionally an yaml formatted configuration file.

        :param path: An optional path to an configuration file
        """
        self.defaults={
            "cache": {
                "use": "internal",
                "redis": {
                    "host": "127.0.0.1",
                    "port": 6379,
                },
                "internal": {
                    "buffer_size": 512, # MB
                }
            }
        }
        self.config=self.defaults

        if path is not None:
            self.load(path)

    def __getitem__(self, item):
        return self.config[item]

    def load(self, path:str):
        """
        Load an yaml formatted configuration file and updates the current
        configuration with its values

        :param path: The path to the configuration file
        """
        with open(path) as f:
            user_config = yaml.safe_load(f)

            # TODO: Merge those dictionaries instead of overwriting
            self.config = user_config

    def save(self, path: str):
        """
        Save the current configuration to the specified path as an
        yaml file

        :param path: The path where the configuration file should be saved
        """
        with open(path, 'w') as f:
            yaml.dump(self.config, f, indent=4)