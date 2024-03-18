#!/bin/bash

# Check for sln file
if [ ! -f "Simulate.FostersAndPartners.sln" ]; then
    echo "Please run the script from the root of the solution."
    exit 1
fi

bash deploy.sh

# Run Postman tests within Docker container
docker build -t simulate-postman:latest -f Dockerfile.postman .

docker run --name integration-tests --network=simulate-network simulate-postman:latest run collection.json --env-var "baseUrl=http://simulate-api:8080"

bash teardown.sh