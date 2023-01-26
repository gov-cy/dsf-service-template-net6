<img src=https://github.com/gov-cy/govdesign/blob/main/dsf-components.png height=75> 

## DSF Components  
Generally, DSF-Ready Services are composed of a Web application (including the DSF Design System and CyLogin integration) and a RESTful API that gets data from and posts data to the Back-end system(s).  The connectivity from the service (which is on the cloud) to the API is implemented through an API Gateway.

## DSF templates to serve as a quickstart guide for developing DSF-ready services:
- **Service template**:This is a quickstart template for creating a sample DSF service (Request an email and mobile change) [git-service-template]
- **RESTful API**: A template project for developing a RESTful API [git-api-template]
- **DSF Design System**: [git-design-system].  Detailed documentation for the DSF Design System is available at: [git-design-system-docs]
- **Sample OIDC Web Client**: to simulate the CyLogin functionality [git-oidc-web-client]. The client is using a mock identity server [dsf-idsrv-dev]


##  **Service template**: This is a quickstart template for creating a sample DSF service (Request an email and mobile change)
This project is an example of a DSF mock service developed in .NET6 Razor Pages

## Why .Net6 Razor Pages
Razor Pages is a newer, simplified web application programming model. It removes much of the ceremony of ASP.NET MVC by adopting a file-based routing approach. Each Razor Pages file found under the Pages directory equates to an endpoint. Razor Pages have an associated C# objected called the page model, which holds each page's behavior. Additionally, each page works on the limited semantics of HTML, only supporting GET and POST methods


## Features
This service is demo that contains the following:
* Implementation of a sample login mechanism with Cy Login 
* A proposed project structure for DSF service development
* Inclusion of the DSF Design System with example pages
* Out-of-the-box Multilanguage Support
* Session data handling and storage
* A proposed server side validation mechanism (Fluent Validation)
* Mock API calls
* Navigation Service

## NuGet Packages
* FluentValidation.AspNetCore
* Microsoft.AspNetCore.Authentication.OpenIdConnect
* Newtonsoft.Json

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
"Oidc:SignedOutRedirectUri": "https://localhost:44319/",
```
- Configure OIDC Authentication in the Program.cs file of the project:
```
builder.Services.AddAuthentication

```

Details of this configuration can be found in the project's `appsettings.json` file and the Program.cs file.
The following test accounts can be used to simulate the login functionality:
`bob/bob, alice/alice, or company1/company1`

We stronly propose to keep sensitive data in secrets, so that values like api key cannot be exposed in gitHub repo

### Note
For real test and production implementations, all the information for the CyLogin OIDC implementation should be provided by the CyLogin team (CyLoginSupport@dits.dmrid.gov.cy)

### Mock API Calls
The project also simulates various API calls to demostrate how the service should get data from and post data to a fictional back-end system  [api-template-demo]. 

## Installation / Running
```
git clone https://github.com/gov-cy/dsf-service-template-net6.git
cd dsf-service-template-net6\src\DSF.AspNetCore.Web.Template
dotnet build
dotnet run
```
### Note
* In order to run git commands, download from [Git]
* if you dont have .Net 6 on you pc download [.Net6-sdk]
* If you are not using Visual Studio run `dotnet restore --source https://api.nuget.org/v3/index.json`
* Defult URL after Build https://localhost:44319/

## Design System

The Service uses css version 1.3.2.
In order to check the latest version, before creating a new service see [css latest]

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
*  Optional Matomo Integration (Statistic Tool use by DSF  )

## License

[MIT License]

**Free Software, Hell Yeah!**

### Non-production Usage. This Software is provided for evaluation, testing, demonstration and other similar purposes. Any license and rights granted hereunder are valid only for such limited non-production purposes.

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
   [.Net6-sdk]: <https://dotnet.microsoft.com/en-us/download/visual-studio-sdks>
   [css latest]: <https://gov-cy.github.io/govcy-design-system-docs/getting_started/>
   [Git]: <https://git-scm.com/downloads> 
