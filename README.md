## Bede.Thallium

A dynamic and extensible ReST client library. Refer to the XML documentation
after compilation for details.

### Examples

#### Basics

Specify a model and interface and then use them:

```C#
public class Ping
{
    public string RespondingHost { get; set; }
    public string Version        { get; set; }
}

public interface IPing
{
    [Get("ping")] Task<Ping> Ping();
}

var sut = Api<IPing>.New(new Uri("http://localhost/api"));

try
{
    var ping = await sut.Ping();
}
catch (HttpRequestException e) // Thrown for error responses
{
    var code    = e.Code();
    var content = e.Content();
}
```

Attach some headers:

```C#
public interface IPingHeader
{
    [Get("ping")] Task<Ping> Ping([Header("myRequestHeader")] string head);
}

var sut = Api<IPingHeader>.New(new Uri("http://localhost/api"));

var rc = (RestClient) sut;

rc.Head["myGlobalHeader"] = "foo";

var ping = await sut.Ping("bar");

```

#### Templates

Level 4 [URI Templates](https://tools.ietf.org/html/rfc6570) are supported,
these examples are not exhaustive:

```C#
public interface ITemplateApi
{
    /*
     * Level 1
     * id1 = 1 and id2 = "two" => "/1/two"
     */
    [Route("/{id1}/{id2}"), ...]
    Task Route(long id1, string id2);

    /*
     * Level 2
     * id = "1/two!" => "/1/two!"
     */
    [Route("/{+id}"), ...]
    Task Route(string id);

    /*
     * Level 3
     * id = 1 and id2 = "two" => "/1/two"
     */
    [Route("{/id1,id2}"), ...]
    Task Route(long id1, string id2);

    /*
     * Level 4
     * match = { { "key1", "foo" }, { "key2", "bar" } } => "?key1=foo&key2=bar"
     */
    [Route("{?match*}"), ...]
    Task Route(IEnumerable<KeyValuePair<string, object>> match);
}
```

#### Generics

Generic interfaces are supported:

```C#
public class MyClass
{
    public long   Id    { get; set; }
    public string Name  { get; set; }
    public int    Value { get; set; }
}

public interface ICrudApi<T>
{
    [Post]           Task<T> Create(T body);

    [Get("{id}")]    Task<T> Read  (long id);

    [Put]            Task<T> Update(T body);

    [Delete("{id}")] Task    Delete(long id);
}

var sut = Api<ICrudApi<MyClass>>(new Uri("http://localhost/api"));

var myClass = await sut.Read(1);
```

#### Composition

Composition is supported:

```C#
public interface IMyApi : IPing, ICrudApi<MyClass> { }
```

#### Multipart content

Sending multipart content is supported:

```C#
public interface IMultipartApi
{
    [Post("{/id*}")]
    [Multipart("form-data", "BOUNDARY")]
    Task Post(string[] id, [FormData("string")]            string                      body1,
                           [FormData("first"), FormUrl]    IDictionary<string, object> body2,
                           [FormData("second", "theFile")] IDictionary<string, object> body3,
                           [FormData, Octet]               FileStream                  theOtherFile);
}
```

#### Fluent Introspection API

A fluent introspection API is provided:

```C#
public interface IFluentApi : IPing, IMultipartApi { }

var sut = Api.Fluent();

sut.Api<IPing>()
   .Get("ping")
   .Method(api => api.Ping());

sut.Api<IMultipartApi>()
   .Post("{/id*}")
   .Multi("form-data", "BOUNDARY")
   .Method(api => api.Post(P.Uri<string[]>(),
                           P.Form<string>("string"),
                           P.Form<Dict>("first").FormUrl(),
                           P.Form<Dict>("second", "theFile"),
                           P.Form<FileStream>().Octet()));

Api<IFluentApi>.Emit(sut);

var client = Api<IFluentApi>.New(new Uri("http://localhost/api"));
```

#### Extensibility

##### Subclassing

You can subclass to augment the base client or modify behaviour, for example to
capture the response message and avoid throws on failure:

```C#
public class TestClient : RestClient
{
    public event EventHandler<HttpResponseMessage> Response;

    public TestClient(Uri uri) : base(uri) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, CancellationToken? token)
    {
        var r = await base.SendAsync(msg, token);

        Response(this, r);

        return r;
    }

    protected override Task Fail(HttpResponseMessage msg)
    {
        return Task.FromResult(true);
    }
}

HttpResponseMessage msg = null;

var sut = Api<TestClient, IPing>.New(new Uri("http://localhost/api"));

var tc = (TestClient) sut;

tc.Response += (o, x) => msg = x;

var ping = await sut.Ping();

Assert.IsNotNull(msg);
```

##### Content Generation

An advanced feature, refer to the source and XML documentation for details of
involved components.

A number of interfaces support content generation, an `IContentBuilder` is
loaded on the stack at the point in the IL stream when the following component
is called:

```C#
public interface IImp
{
    void Process(ILGenerator ilG, Description call);
}

Api.Imp = new MyImp();
```


