[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/lord-executor/Ninject.Web.AspNetCore/blob/master/LICENSE) [![Build Status](https://travis-ci.org/lord-executor/Ninject.Web.AspNetCore.svg?branch=master)](https://travis-ci.org/lord-executor/Ninject.Web.AspNetCore)

# Configuration
`Ninject.Web.AspNetCore` is built in a way that makes it easy to integrate with all standard ASP.NET Core examples and templates. All you have to do is eliminate the default web host builder and use the `AspNetCoreHostConfiguration` in combination with the `NinjectSelfHostBootstrapper` instead like this.

```
public class Program
{
    public static void Main(string[] args)
    {
        var hostConfiguration = new AspNetCoreHostConfiguration(args)
                .UseStartup<Startup>();
                .UseKestrel();

        var host = new NinjectSelfHostBootstrapper(CreateKernel, hostConfiguration);
        host.Start();
    }

    public static IKernel CreateKernel()
    {
        var settings = new NinjectSettings();
        // Unfortunately, in .NET Core projects, referenced NuGet assemblies are not copied to the output directory
        // in a normal build which means that the automatic extension loading does not work _reliably_ and it is
        // much more reasonable to not rely on that and load everything explicitly.
        settings.LoadExtensions = false;

        var kernel = new StandardKernel(settings);

        kernel.Load(typeof(AspNetCoreHostConfiguration).Assembly);

        return kernel;
    }
}
```

Then, for your `Starup` configuration you will have to inherit from `AspNetCoreStartupBase`. Use the sample below and add your custom configuration in the marked locations.

```
public class Startup : AspNetCoreStartupBase
{
    public Startup(IConfiguration configuration, IServiceProviderFactory<NInjectServiceProviderBuilder> providerFactory)
        : base(providerFactory)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        // Add your services configuration HERE
    }

    public override void Configure(IApplicationBuilder app)
    {
        // For simplicitly, there is only one overload of Configure supported, so in order to get the additional
        // services, you can just resolve them with the service provider.
        var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
        // For ASP.NET Core 3+ you can instead resolve IWebHostEnvironment
        // var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        // Add your application builder configuration HERE
    }
}
```

## Using your own IWebHostBuilder
Of course, you can still use your own web host builder if your application requires that. You can simply call `UseWebHostBuilder` on the `AspNetCoreHostConfiguration` to provide your own `IWebHostBuilder` like so.

```
public class Program
{
    public static void Main(string[] args)
    {
        var hostConfiguration = new AspNetCoreHostConfiguration(args)
                .UseWebHostBuilder(CreateWebHostBuilder)
                .UseStartup<Startup>();
                .UseKestrel();

        var host = new NinjectSelfHostBootstrapper(CreateKernel, hostConfiguration);
            host.Start();
    }

    private static IWebHostBuilder CreateWebHostBuilder()
    {
        return new WebHostBuilder()
            // ...
    }
}
```

You can also use `DefaultWebHostConfiguration` which comes with the Ninject integration to use some of the defaults that you get with an OOTB default web host builder for ASP.NET and add your own configuration on top of that. `DefaultWebHostConfiguration` can essentially do the same as `WebHost.CreateDefaultBuilder` except that it does not already configure the actual hosting model. It just adds the default configuration for logging, content root, etc.

```
public class Program
{
    public static void Main(string[] args)
    {
        var hostConfiguration = new AspNetCoreHostConfiguration(args)
                .UseWebHostBuilder(CreateWebHostBuilder)
                .UseStartup<Startup>();
                .UseKestrel();

        var host = new NinjectSelfHostBootstrapper(CreateKernel, hostConfiguration);
            host.Start();
    }

    private static IWebHostBuilder CreateWebHostBuilder()
    {
        return new DefaultWebHostConfiguration()
            .ConfigureContentRoot()
            .ConfigureAppSettings()
            .ConfigureLogging()
            .ConfigureAllowedHosts()
            // .ConfigureForwardedHeaders() // ASP.NET Core 3+ only
            .ConfigureRouting()
            .GetBuilder();
    }
}
```

# The Problem with WebHost.CreateDefaultBuilder
In most ASP.NET Core examples that you find out there, including the ones you get when creating a new project from one of the many templates, you will find code like the following:

```
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
```

That default web host builder already registers both Kestrel and IIS servers. This is somewhat stupid in several different ways:
* Both Kestrel and IIS integration have already been configured before you, the developer, have declared which hosting model you would like to use.
* Calling `UseKestrel` or `UseIIS` without additional configuration are essentially NO-OPs.

The biggest problem however is this: `UseIIS` only really has an effect when the assemlby is actuall being executed inside of an IIS (w3wp) process. In that case, it will register its own `IServer` implementation with the `ServiceCollection`. Unfortunately, this means that because Kestrell is also configured and already registered its own `IServer` instance, there are now **two** `IServer` implementations. With the _cheap_ `ServiceProvider` implementation that comes with ASP.NET Core this is not a problem because when a _single_ service is requested, it will just return the _last_ implementation that was registered.

When these service registrations are translated to Ninject, this becomes a bit of a problem since Ninject won't be able to resolve a single service if there are multiple applicable bindings. Since the `IServiceProvider` implementation should return `null` if it cannot resolve a service, that is exactly what happens and the startup will fail because it cannot find an `IServer` implementation.

The same thing of course happens if you are using the default web host builder _and then_ tell it to `UseHttpSys`... Because it has already registered Kestrel (and IIS, but I'm assuming that the application is running in its own process here) it will still have two `IServer` implementations and startup will fail.

This is why with `Ninject.Web.AspNetCore` the `AspNetCoreHostConfiguration` only expects you to explicitly declare the hosting model and only allows you to define _one_ hosting model. If you try to register multiple ones, it will simply use the last one that was configured. This is due to the fact that internally `AspNetCoreHostConfiguration` just stores one `_hostingModelConfigurationAction`.


# Detecting IIS In-Process Hosting
If you want you server to be able to run as a separate stand-alone server _as well as_ integrated into an IIS server with in-process hosting, then your code needs to be able to call the right hosting model method (`UseKestrel` or `UseIIS`) depending on the context.

You can check the SampleApplication `Program.cs` for practical examples.

## Environment Variables
The built-in ASP.NET Core IIS integration detection code is relying on OS detection and Windows specific kernel methods, but there is a much cleaner way to do that. A mechanism that has been used to detect the runtime context of an application since the dawn of modern operating systems: **environment variables**.

For .NET Core 2.2 there already seems to be an environment variable defined by the IIS AspNetCore integration that makes this quite easy:
```
ASPNETCORE_HOSTINGSTARTUPASSEMBLIES = Microsoft.AspNetCore.Server.IISIntegration
```
Unfortunately that only works for .NET Core 2.2 and the variable no longer exists with higher versions. I'm sure that you can pick an environment variable for other versions of .NET Core as well, but there does not seem to be anything _consistent_.

If that is not _reliable_ enough, you can also simply introduce your own environment variable in the IIS configuration as described in https://www.andrecarlucci.com/en/setting-environment-variables-for-asp-net-core-when-publishing-on-iis/. The sample application checks for a `SERVER_HOSTING_MODEL` environment variable that contains the name of the hosting model to use - for the IIS in-process configuration this would simply be set to `IIS`.

## Using Command Line Arguments
In the `web.config` for your IIS integration which may be generated for you when you publish the project to IIS, you can simply modify the command arguments like shown below for an in-process IIS hosting.
```
<aspNetCore processPath="dotnet" arguments=".\SampleApplication-AspNetCore22.dll --useIIS" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="InProcess" />
```

If you want to host the server as an out-of-process self-hosted application, then you can do something like this instead.
```
<aspNetCore processPath="dotnet" arguments=".\SampleApplication-AspNetCore22.dll --useKestrel" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="OutOfProcess" />
```


# Versioning
The package version numbers are chosen to align with the version of ASP.NET Core they were built against.

* Version 2.2.0 is compatible with ASP.NET Core 2.2.0 and is compatible with .NET Standard 2.0. This means that it is the only version that works for both .NET Core projects as well as .NET 4.8 projects.
