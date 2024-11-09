# Fintrellis Blogging Web API

A simple web api managing the CRUD operation for posts for a blogging service.

# Contents

- [Getting Started](#getting-started)
  - [High level folder structure](#high-level-folder-structure)
  - [Prerequisites](#prerequisites)
  - [Installing](#installing)
  - [Running tests](#running-tests)
- [About the Application](#about-the-application)
  - [Structure](#structure)
  - [External Libraries](#external-libraries)
- [Authors](#authors)

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### High level folder structure

- **Fintrellis.WebApi/**

  - **local/**
    - `docker-compose.yml`
  - **src/**

    - `Dockerfile`

    - **Fintrellis.Services/**
    - **Fintrellis.Redis/**
    - **Fintrellis.MongoDb/**
    - **Fintrellis.WebApi/**
    - **Fintrellis.Services.Tests/**
    - `Fintrellis.WebApi.sln`

  - `README.md`

### Prerequisites

- Docker Desktop to run the application as a container
- Visual Studio (optional) to debug or execute tests

### Installing

Clone the repo at your desired location:

```
git clone https://github.com/FarazPoomun/Fintrellis.WebApi.git
```

Within the clone folder, navigate to the **local/** folder as shown in the [High level folder structure](#high-level-folder-structure) and open a terminal in that location.  
A docker compose file will be present that is configured to have the application, mongo db and redid up and running on your machine.  
Run the following command in the terminal to get the containers running:

```
docker compose up -d
```

The application will be running locally on port 8063. By default, opening [Localhost at port 8063](http://localhost:8063/) is configured to take you to the Swagger page.

### Running tests

Running the unit tests of the applications can either be achieved via

- Manually running all tests from Visual Studio
- Or using terminal, within the /src/ folder run `dotnet test`

## About the Application

This section is to explain about the application and features of the application.

### Structure

The application is broken down into 4 projects.

- Fintrellis.WebApi (.Net Core Web API)

  - This project is the entry point of the application.
  - It holds the controller and is responsible for setting up the dependency injection container for the entire application.
  - It also reads configurations from configuration file and utilize them accordingly.

- Fintrellis.Services (Class Library)

  - This project holds the necessary business logic that is consumed by the controller.
  - It also includes retry policy functionality for when exeptions are thrown by outside code.
  - Uses redis to cache data for faster retrieval.
  - Validators also resides within this project to validate user inputs.

- Fintrellis.MongoDb (Class Library)

  - This project holds the necessary code to communicate with MongoDb.
  - It uses the repository design pattern to provide a generic repository class that can be consumed by services to do CRUD operation on the database.
  - Also defines a contract `IMongoEntity.cs` that all entity should abide by in order to properly utilize the repository class.

- Fintrellis.Redis (Class Library)

  - This project holds the necessary code to configure Redis.
  - It uses the repository design pattern to provide a generic class to manipulate cache data.

- Fintrellis.Services.Tests(xUnit Test Project)
  - This project holds the tests to test the business logic of the application, more specifically testing Fintrellis.Services project.

### External Libraries

- FluentValidation: Provides a fluent interface for validating request data, ensuring user inputs are accurate before processing.
- MongoDB.Driver: Facilitates seamless interaction with MongoDB as a data store, using efficient CRUD operations.
- AutoMapper: Simplifies mapping between different object models, e.g., mapping between request models and entity models.
- Polly: Implements retry policies to handle transient faults and improve application resilience, especially during database interactions.

## Author

- **Faraz Poomun** - [Read About Me 🤖](https://farazpoomun.com/)
