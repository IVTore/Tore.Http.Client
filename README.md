# Tore.Http.Client
Http utilities library for C# By İ. Volkan Töre.

Language: C#.

Nuget package: [Tore.Http.Client](https://www.nuget.org/packages/Tore.Http.Client/)

[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://choosealicense.com/licenses/mit/)

Dependencies: <br/>
    net 7.0
    Tore.Core 8.0.0+

Changes:
    
    This was named Com.cs and was a part of Tore.Core library.
    So the version starts with 8.0.0.
    It is now a seperate .net 7.0 library with a nuget package.


## Client.cs :
Defines the Client class which manages Http client requests and responses.
Client is an all in one Http Client object gathering sub components required for
a proper request and its response into an instance.
Since client requests and responses differ dramatically, 
Client objects give both standard and micro managed request types and response handling.
It maintains and uses a single static HttpClient object as recommended by Microsoft.

For simple standard http requests use STATIC functions 

```C#
   Client.Send(...);
   Client.SendAsync(...);
   Client.Talk<T>(...);
   Client.TalkAsync<T>(...);
```
Otherwise, create a Client object and manipulate the request via

   - The Client instance properties, like: content, accept, mediaType.
   - The Client.request, the HttpRequestMessage property, directly.

Then use instance send() or sendAsync() routines. 
  
IMPORTANT:
Instance content, accept and mediaType properties are transferred to request just before sending it.
Tricky assignments must be done via;
   - request.Content,
   - request.Content.Headers.Accept,
   - request.Content.Headers.ContentType.MediaType properties,
  
When request.content is non null, content property of instance is ignored.
If accept or mediaType is set through request.Content.Headers
respective Com accept and mediaType properties must be empty.  

Please read the comments on the code at least once for using this class efficiently.
