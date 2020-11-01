[![Build Status](https://travis-ci.org/lord-executor/Ninject.Web.AspNetCore.svg?branch=master)](https://travis-ci.org/lord-executor/Ninject.Web.AspNetCore)

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
        var kernel = new StandardKernel();
        return kernel;
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
