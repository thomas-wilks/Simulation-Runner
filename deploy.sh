#!/bin/bash

# Check for sln file
if [ ! -f "Simulate.FostersAndPartners.sln" ]; then
    echo "Please run the script from the root of the solution."
    exit 1
fi

bash teardown.sh

# Setup Docker Network
docker network create simulate-network

# Run RabbitMQ Container
docker run -it -d --name rabbitmq --network=simulate-network -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management

# Run MongoDB Container
docker run -it -d --name mongocontainer --network=simulate-network -p 27017:27017 mongo:latest

# Sleep to allow RabbitMQ and MongoDB to fully start
sleep 5

# Build and run worker pod
docker build -t simulate-worker:latest -f Dockerfile.worker .
docker run -it -d --name simulate-worker --network=simulate-network --env-file .env.example simulate-worker:latest

# Build and run api pod
docker build -t simulate-api:latest -f Dockerfile.api .
docker run -it -d --name simulate-api --network=simulate-network --env-file .env.example -p 8080:8080 -p 8443:8443 simulate-api:latest

