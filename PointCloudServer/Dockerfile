FROM python:3.9-bullseye

# Setup PointCloudServer
COPY . /app
WORKDIR /app
RUN pip install .

# Setup traffic shaping environment
RUN apt-get update
RUN pip install  tcconfig
RUN apt-get install -y iproute2 iptables

# Start the container
EXPOSE 8080
ENTRYPOINT pointcloudserver dash --config configuration.yaml --host 0.0.0.0 --port 8080 --mediaDir media/