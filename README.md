# Fluff Rest : Simple Fluent .NET HTTP Client Wrapper

## Features

* Easy HTTP calls with JSON serialization and deserialization with `System.Text.Json`.
* Simple Endpoint selection.
* All http verbs.
* Simple auth configuration with basic and bearer with support for custom schemes.
* Query parameter serialization to http url format.
* Easy header configuration.
* Fully customizable pipeline with listeners that can change the behaviour as desired.
* Automatic cancellation of requests that is configurable.

## Contributing

You are welcomed to create a PR so we can discuss about it. Or open issues.

If you make a PR please **write tests** and make sure existing tests **pass**. also please not that you need to achieve 95% coverage for your PR to be merged. There is a script in the sources to run a coverage report to help you.

## Inspiration

This lib was heavily influenced by [Tiny Rest Client](https://github.com/jgiacomini/Tiny.RestClient) made by [Jérôme GIACOMINI](https://github.com/jgiacomini).

## Basic examples

**For more details please consult the wiki**

### Basic query

```csharp
var httpClient = new HttpClient(); // You can create it or get it from a factory.

// We need a client, the url is the root url of the api, we will specify endpoint later.
var fluffClient = new FluffRestClient("https://api.example.com/v1", httpClient);

// Will get : https://api.example.com/v1/endpoint and try to deserialize the JSON into the object MyDto.
// Note that if the type parameter is omitted it will not read the result.
MyDto result = await fluffClient.Get("endpoint").ExecAsync<MyDto>();
```

### Body

```csharp
// Will post : https://api.example.com/v1/endpoint with json of the object MyDto.
var dto = new MyDto() { ... };
var result = await fluffClient.Post("endpoint")
    .AddBody(dto)
    .ExecAsync();
```

### Query Params

```csharp
// Will get : https://api.example.com/v1/endpoint?id=1&name=Test
var result = await fluffClient.Get("endpoint")
    .AddQueryParameter("id", 1)
    .AddQueryParameter("name", "Test")
    .ExecAsync();
```

### Headers

You can setup headers on the client to be set globally for each request made with the client.

```csharp
// Will get : https://api.example.com/v1/endpoint with header "x-demo" with value "value". 
fluffClient = fluffClient.AddDefaultHeader("x-demo", "value");
var result = await fluffClient.Get("endpoint").ExecAsync();
```

Or it can be set for one request

```csharp
// Will get : https://api.example.com/v1/endpoint with header "x-demo" with value "value". 
var result = await fluffClient.Get("endpoint")
    .AddHeader("x-demo", "value")
    .ExecAsync();
```

### Automatic cancellation

There is two behaviours for the client at the moment :

* `PerEndpoint` : Meaning that if you make two request to the same endpoint the first one will be cancelled. But only if it's the same endpoint. Concurrent request to different endpoints will not trigger the cancellation of the olders.
* `PerClient` : Meaning that every new request will cancel the previous one. Concurrent request to different endpoints will trigger the cancellation of the olders.

```csharp
// To enable it
fluffClient = fluffClient.WithAutoCancellation();
```

To configure the desired behaviour please see configuration in the wiki.

You can also cancell all request of a client

```csharp
fluffClient.CancellAllRequests();
```
