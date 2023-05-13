## Demo Minimal API

To run this project, just download it and run the EntityFramework migration and also change the database ConnectionString.

In this project there is an entity called Supplier and the app has some endpoints like GetAll, GetById, Create, Update and Delete to deal with the database records for this entity.

This project uses Authentication and Authorization features with Bearer token JWT.
It's possible go generate a token by the Login endpoint and also check if the user is authenticated.

Some endpoints are open to everyone, some of them need to have an Admin profile and others can be accessed with an Employee profile.

The goal of this project is to address the differences between APIs with controllers using MVC and Minimal APIs

It was used FluentValidation package and the database is SQLServer.