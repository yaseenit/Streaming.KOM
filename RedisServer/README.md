# Redis Server
A simple docker-compose setup to run a Redis server.

## Requirements
- Docker
- docker-compose

## Usage
Settings can be customized in `docker-compose.yaml`. To start the container run from this folder:
```bash
docker-compose up -d
```

The container will start now and is ready to accept connections. You can stop the server again with:
```bash
docker-compose down
```

Further instructions to customize the container can be found at the official repository of the image: [https://hub.docker.com/r/bitnami/redis/](https://hub.docker.com/r/bitnami/redis/)