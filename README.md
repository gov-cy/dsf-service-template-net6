# dsf-service-template-net6
## A quickstart template for creating a sample DSF service (Request an address change)
This project is an example of a DSF mock service developed in .NET6 Razor Pages  

## Features
This service is demo that contains the following:
* Implementation of a sample login mechanism with Cy Login
* A proposed project structure for DSF service development
* Inclusion of the DSF Design System with example pages (see layout Page)
* Out-of-the-box Multilanguage Support
* Session data handling and storage
* A proposed server side validation mechanism (Fluent Validation)
* Mock API calls
* Sample Unit Tests
* much more to come ...

## Integrations

### CyLogin Mock Integration
This project simulates CYLogin functionality by using a mock IdentityServer.
Usage:
- Configure OIDC settings in a configuration file:
```
"Oidc:Scopes": "<scopes>",
"Oidc:RedirectUri": "https://localhost:44319/signin-oidc",
"Oidc:ClientSecret": "<password>",
"Oidc:ClientId": "<client_id>",
"Oidc:Authority": "<authority_url>",
"RedirectUri": "https://localhost:44319/",
```
- Configure OIDC Authentication in the Program.cs file of the project:
```
builder.Services.AddAuthentication

```

Details of this configuration can be found in the project's `appsettings.json` file and the Program.cs file.
The following test accounts can be used to simulate the login functionality:
`bob/bob, alice/alice, or company1/company1`

### Mock API Calls
The project also simulates various API calls to demostrate how the service should get data from and post data to a fictional back-end system. 

## Installation
```
git clone https://github.com/gov-cy/dsf-service-template-net6.git
cd src\dsf-service-template-net6
dotnet build
dotnet run
```

## Tech
* Localization
* Oidc Authentication (Mock CyLogin) [dsf-idsrv-dev]
* Razor Pages Authentication (Startup configuration)
* Dependency Injection
* Server side validation
* Properties Binding
* Session Management
* API Calls [dsf-mock-apis]

## License

MIT

**Free Software, Hell Yeah!**

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job.)

   [dsf-idsrv-dev]: <https://dsf-idsrv-dev.dmrid.gov.cy>
   [dsf-mock-apis]: <https://dsf-api-test.dmrid.gov.cy/index.html>
   [git-repo-url]: <https://github.com/gov-cy/dsf-service-template-net6.git>
   
