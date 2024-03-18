# Design Assumptions

1. An internal use API to allow users to queue long running simulations, the API should provide a way for the user to check on the status of their simulation as well as get the final result.

2. Queue can be implemented in a FIFO manner. There is no need to implement a priority system.

3. Simulate method can be a shorter number of seconds as proof of concept. For the sake of integration tests I will make this range 15-30 seconds.

4. No need for scaling, single API and Worker pods. Queue does not need to handle multiple consumers.

5. No auth requirement in this iteration.

6. Worker should be non-blocking but the number of simulations being processed should be bound in some way, I will set the max number of concurrent simulations to 5.

7. The worker is to perform the simulation method run as well as message handling.

8. User should not be able to cancel a simulation once it has been queued.

# Implementation Pitfalls

1. API requires polling to check when a simulation is finished, provided more time I would implement a way for the requesting client to be informed of the progress and finishing of their queued simulation. Most likely I would implement a pub/sub system such as Azure SignalR to allow the client to receive updates directly. Depending on the length of the run as well as the expected size of the queue email updates could also be utilised.

2. Design is reliant on a database for storage, this would not work well if the simulation produced files that the user may wish to download and view. Some form of file store would need to be injected to deal with this.

3. The simulation queue is a FIFO model, this does not utilise the dummy "priority" field I added to the request. Given a full implementation the waiting queue should be set in priority order. Alongside this, it may be beneficial to have some form of estimate for how long a simulation will run for and some logic to ensure that a short run does not get stuck behind a large number of extremely long runs.

4. Simulation error handling does not exist, on failure it is not requeued to try again etc. Given more time this could be something that it could benefit from.

5. There is no way to check what number in the queue your simulation is, once it is queued you just have to wait for it to process. Having an API section that would allow you to see where your simulation is in relation to existing running simulations and the queue could be benifical. This would need to take into account any data privacy requirements.

6. As the Worker pod carries out the "dummy" simulated work there would need to be some form of scaling method implemented to allow large queues to be worked through in a more timely manner. The implemented worker allows for 5 concurrent simulations to run, in a real implementation this may be a 1-to-1 relationship. Hardware provisioning and availability would be a key element to this. This scaling change may require an additional service designed to send queued requests to specific machines for processing.

7. There is no way for a user to cancel a queued simulation or amend its parameters before it starts. The API could easily be changed to allow the user to amend the data being held for the simulation run. Ensuring that the simulation request is put into a "hold" state so that it is not started while data is being changed would be important.

# API Documentation
### POST /api/simulation
Submit a new simulation for computation

**Request**
```
{
    "priority": <int>
}
```
**Validation**
- Priority must be in the range (1,5)

**Response**
- 200: OK
```
{
    "id": <string>
}
```
- 400 Bad Request

### GET /api/simulation/{id}/status
Get the status of your queued simulation request

**Validation**
- Id must be non-empty and an ObjectId

**Response**
- 200: OK
```
{
    "status": "Queued" | "Started" | "Completed",
    "queuedAt": <DateTime>,
    "startedAt": null | <DateTime>,
    "completedAt": null | <DateTime>
}
```
- 400: Bad Request
- 404: Not Found

### GET /api/simulation/{id}/result
Get the result of a finished simulation

**Validation**
- Id must be non-empty and an ObjectId
- Simulation Status must be "Completed"

**Response**
- 200: OK
```
{
    "data": <string>
}
```
- 400: Bad Request
- 404: Not Found


# External Library Choices

### RabbitMQ
A free and open source message broker, chosen for the ability to self host within docker and create a complete deployment within the single repository. Due to the need to only implement a FIFO it also seemed like the quickest choice to get setup within the code, RabbitMQ allows many more configuration options too so could be extended with the codebase.

### MongoDB
Chosen again due to ability to self host and NoSQL chosen due to the data requirements being low for this proof of concept so implementation would be much faster. SQL seemed overkill for the small amount of data that was being stored. If the simulation run in a full system produced data that could be stored in a database then this may need to be revisited.

# Dependencies
- .NET 8.0 (Unit tests only)
- Docker
- Unix based OS for deployment scripts

# Testing Instructions

### Unit Tests
From the root directory of the project:

`$ cd xunit`

`$ dotnet test`

The unit tests ensure that the controller functions return the expected HTTP responses given certain inputs.

# Integration Tests
From the root directory of the project:

NOTE: One test will halt and wait 30 seconds to give the worker time to process the simulation run

`$ ./test.sh`

# Running Instructions
If you wish to run the stack you can do so using the deploy script from the root of the project:

`$ deploy.sh`

The API will then be available at `http://localhost:8080/` this can be set as the `baseUrl` within the Postman collection.

To delete all the containers you can run `$ teardown.sh`