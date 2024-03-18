#!/bin/bash

# Delete docker containers if they exist
docker rm -f simulate-api
docker rm -f simulate-worker
docker rm -f mongocontainer
docker rm -f rabbitmq
docker rm -f integration-tests

# Delete docker network
docker network rm simulate-network