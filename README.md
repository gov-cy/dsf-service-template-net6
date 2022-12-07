
# dsf-service-template-net6
<img src=https://github.com/gov-cy/govdesign/blob/main/dsf-components.png height=75> 
## DSF Components  
Generally, DSF-Ready Services are composed of a Web application (including the DSF Design System and CyLogin integration) and a RESTful API that gets data from and posts data to the Back-end system(s).  The connectivity from the service (which is on the cloud) to the API is implemented through an API Gateway.

## DSF templates to serve as a quickstart guide for developing DSF-ready services:
- **RESTful API**: This repository provides a template project for developing a RESTful API [git-api-template]
- **DSF Design System**: [git-design-system].  Detailed documentation for the DSF Design System is available at: [git-design-system-docs]
- **Service template**: A service example to provide a template for a dsf-ready service including the DSF Design system and basic functionality [git-service-template]
- **Sample OIDC Web Client**: to simulate the CyLogin functionality [git-oidc-web-client]. The client is using a mock identity server [dsf-idsrv-dev]


## This is a quickstart template for creating a sample DSF service (Request an email and mobile change)
This project is an example of a DSF mock service developed in .NET6 Razor Pages

## Why .Net6 Razor Pages
Razor Pages is a newer, simplified web application programming model. It removes much of the ceremony of ASP.NET MVC by adopting a file-based routing approach. Each Razor Pages file found under the Pages directory equates to an endpoint. Razor Pages have an associated C# objected called the page model, which holds each page's behavior. Additionally, each page works on the limited semantics of HTML, only supporting GET and POST methods


## Features
This service is demo that contains the following:
* Implementation of a sample login mechanism with Cy Login 
* A proposed project structure for DSF service development
* Inclusion of the DSF Design System with example pages (see layout Page)
* Out-of-the-box Multilanguage Support
* Session data handling and storage
* A proposed server side validation mechanism (Fluent Validation)
* Mock API calls
* Navigation Service


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
The project also simulates various API calls to demostrate how the service should get data from and post data to a fictional back-end system  [api-template-demo]. 

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
* Server side validation [Fluent-url]
* Properties Binding
* Session Management
* API Calls [dsf-mock-apis]

## Other
* Matomo Integration

## License

MIT

**Free Software, Hell Yeah!**

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job.)
   [git-api-template]: <https://github.com/gov-cy/dsf-api-template-net6.git>
   [api-template-demo]: <https://dsf-api-test.dmrid.gov.cy/swagger/index.html>
   [git-service-template]: <https://github.com/gov-cy/dsf-service-template-net6>
   [git-design-system]: <https://github.com/gov-cy/govcy-design-system>
   [git-design-system-docs]: <https://gov-cy.github.io/govcy-design-system-docs/>
   [git-oidc-web-client]: <https://github.com/gov-cy/dsf-oidc-web-client> 
   [dsf-idsrv-dev]: <https://dsf-idsrv-dev.dmrid.gov.cy>
   [dsf-mock-apis]: <https://dsf-api-test.dmrid.gov.cy/swagger/index.html>
   [git-repo-url]: <https://github.com/gov-cy/dsf-service-template-net6.git>
   [Fluent-url]: <https://docs.fluentvalidation.net/en/latest/>
