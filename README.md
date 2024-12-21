[![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)](https://github.com/lord-executor/Ninject.Web.AspNetCore/blob/master/LICENSE) [![.NET 8 CI](https://github.com/lord-executor/Ninject.Web.AspNetCore/actions/workflows/ci.yml/badge.svg)](https://github.com/lord-executor/Ninject.Web.AspNetCore/actions/workflows/ci.yml)


# Overview
This project provides full [Ninject](https://github.com/ninject/Ninject) integration with ASP.NET Core projects. Full integration means that the Ninject kernel is used to replace the standard service provider that comes with ASP.NET Core.

The project consists of three different NuGet packages:
* [Ninject.Web.AspNetCore](https://www.nuget.org/packages/Ninject.Web.AspNetCore/) - This provides all that is needed to use Ninject in an ASP.NET core project except for the configuration of Windows specific hosting models
* [Ninject.Web.AspNetCore.IIS](https://www.nuget.org/packages/Ninject.Web.AspNetCore.IIS/) - Provides a simple way to configure the project to run in IIS.
* [Ninject.Web.AspNetCore.HttpSys](https://www.nuget.org/packages/Ninject.Web.AspNetCore.HttpSys/) - Provides a simple way to configure the project to run with the low-level HTTPSys webserver.

The IIS and HttpSys projects are just very small convenience wrappers and you can just as easily copy the handful of lines of configuration code to your own project if you don't want to add them as dependencies.


# Configuration
`Ninject.Web.AspNetCore` is built in a way that makes it easy to integrate with all standard ASP.NET Core examples and templates. All you have to do is eliminate the default web host builder and use the `AspNetCoreHostConfiguration` in combination with the `NinjectSelfHostBootstrapper` like this.

```cs
using Microsoft.AspNetCore.Hosting;
using Ninject;
using Ninject.Web.AspNetCore;
using Ninject.Web.AspNetCore.Hosting;
using Ninject.Web.Common.SelfHost;

public class Program
{
    public static void Main(string[] args)
    {
        var hostConfiguration = new AspNetCoreHostConfiguration(args)
                .UseStartup<Startup>()
                .UseKestrel()
                .BlockOnStart();

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

        var kernel = new AspNetCoreKernel(settings);

        kernel.Load(typeof(AspNetCoreHostConfiguration).Assembly);

        return kernel;
    }
}
```

Then, for your `Starup` configuration you will have to inherit from `AspNetCoreStartupBase`. Use the sample below and add your custom configuration in the marked locations.

```cs
public class Startup : AspNetCoreStartupBase
{
    public Startup(IConfiguration configuration, IServiceProviderFactory<NinjectServiceProviderBuilder> providerFactory)
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
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        // Add your application builder configuration HERE
    }
}
```

## Using your own IWebHostBuilder
Of course, you can still use your own web host builder if your application requires that. You can simply call `UseWebHostBuilder` on the `AspNetCoreHostConfiguration` to provide your own `IWebHostBuilder` like so.

```cs
public class Program
{
    public static void Main(string[] args)
    {
        var hostConfiguration = new AspNetCoreHostConfiguration(args)
                .UseWebHostBuilder(CreateWebHostBuilder)
                .UseStartup<Startup>();
                .UseKestrel()
                .BlockOnStart();

        var host = new NinjectSelfHostBootstrapper(CreateKernel, hostConfiguration);
            host.Start();
    }

    public static IWebHostBuilder CreateWebHostBuilder()
	{
		return new DefaultWebHostConfiguration(null)
			.ConfigureAll()
			.GetBuilder();
	}
}
```

You can also use `DefaultWebHostConfiguration` which comes with the Ninject integration to use some of the defaults that you get with an OOTB default web host builder for ASP.NET and add your own configuration on top of that. `DefaultWebHostConfiguration` can essentially do the same as `WebHost.CreateDefaultBuilder` except that it does not already configure the actual hosting model. It just adds the default configuration for logging, content root, etc.

```cs
using Microsoft.AspNetCore.Hosting;
using Ninject;
using Ninject.Web.AspNetCore;
using Ninject.Web.AspNetCore.Hosting;
using Ninject.Web.Common.SelfHost;

public class Program
{
    public static void Main(string[] args)
    {
        var hostConfiguration = new AspNetCoreHostConfiguration()
                .UseWebHostBuilder(CreateWebHostBuilder(args))
                .UseStartup<Startup>();
                .UseKestrel()
                .BlockOnStart();

        var host = new NinjectSelfHostBootstrapper(CreateKernel, hostConfiguration);
            host.Start();
    }

    public static IKernel CreateKernel()
    {
        // ...
    }

    private static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
        return new DefaultWebHostConfiguration(args)
            .ConfigureContentRoot()
            .ConfigureAppSettings()
            .ConfigureLogging()
            .ConfigureAllowedHosts()
            .ConfigureForwardedHeaders()
            .ConfigureRouting()
            .GetBuilder();
    }
}
```


# The Problem with WebHost.CreateDefaultBuilder
In most ASP.NET Core examples that you find out there, including the ones you get when creating a new project from one of the many templates, you will find code like the following:

```cs
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
```

That default web host builder already registers both Kestrel and IIS servers. This is somewhat stupid in several different ways:
* Both Kestrel and IIS integration have already been configured before you, the developer, have declared which hosting model you would like to use.
* Calling `UseKestrel` or `UseIIS` without additional configuration are essentially NO-OPs.

The biggest problem however is this: `UseIIS` only really has an effect when the assemlby is actually being executed inside of an IIS (w3wp) process. In that case, it will register its own `IServer` implementation with the `ServiceCollection`. Unfortunately, this means that because Kestrel is also configured and already registered its own `IServer` instance, there are now **two** `IServer` implementations. With the _cheap_ `ServiceProvider` implementation that comes with ASP.NET Core this is not a problem because when a _single_ service is requested, it will just return the _last_ implementation that was registered.

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

If that is not _reliable_ enough, you can also simply introduce your own environment variable in the IIS configuration as described in https://docs.microsoft.com/en-us/iis/configuration/system.applicationhost/applicationpools/add/environmentvariables/. All you really need to do is update the relevant application pool in your `%WinDir%\System32\Inetsrv\Config\applicationHost.config` to something like this:

```
<applicationPools>
	<add name="MyAppPool">
		<environmentVariables>
			<add name="SERVER_HOSTING_MODEL" value="IIS" />
		</environmentVariables>
	</add>
</applicationPools>
```

The sample application [Program.cs](src/SampleApplication-AspNetCore22/Program.cs) checks for a `SERVER_HOSTING_MODEL` environment variable that contains the name of the hosting model to use - for the IIS in-process configuration this would simply be set to `IIS`.

## Using Command Line Arguments
In the `web.config` for your IIS integration which may be generated for you when you publish the project to IIS, you can simply modify the command arguments like shown below for an in-process IIS hosting.
```
<aspNetCore processPath="dotnet" arguments=".\SampleApplication-AspNetCore22.dll --useIIS" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="InProcess" />
```

If you want to host the server as an out-of-process self-hosted application, then you can do something like this instead.
```
<aspNetCore processPath="dotnet" arguments=".\SampleApplication-AspNetCore22.dll --useKestrel" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="OutOfProcess" />
```


# Compliance with Microsoft.Extensions.DependencyInjection
In order to assess the compatibility of any third-party DI container with the assumptions of `Microsoft.Extensions.DependencyInjection`, there is a "specification" test project called [Microsoft.Extensions.DependencyInjection.Specification.Tests](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions.DependencyInjection.Specification.Tests/src)

## What is Different?
There are some subtle and some not so subtle differences between the assumptions that Ninject makes compared to Microsoft DI. To get Ninject to be compatible with those assumptions, some "tweaks" had to be introduce that _may_ affect your code if you are unaware of them. This section tries to explain the important differences and how they might affect your application.

### Allow Null Injection
The `AspNetCoreKernel` sets the `INinjectSettings.AllowNullInjection` property to `true` since that is a use case that is used in some ASP.NET Core scenarios. Since this setting can only be set _globally_ for the kernel, your application has to live with that too.

### Scopes
Scopes in Microsoft DI work quite different than they do in Ninject.
* Transient services are instantiated whenever one is requested as you might expect, but their _disposal_ is directly tied to the service provider scope in which they were created. This means for example that transient services that are created throught the `IServiceProvider` during a request are tied to the scope that is created for each request and they are disposed at the end of that request. This leads into the disposal ordering difference described below.
* Scoped services are always tied to the `IServiceScope` in which they were created. By default that is the root service provider, but new scopes can be created and destroyed at any time through the `IServiceScopeFactory`.
* Singleton services are tied to the root service provider. This is actually equivalent to Ninject which ties singletons to the kernel.

To compensate for those differences, services that are registered through the `IServiceCollection` are treated differently from services that are registered directly with the kernel. When a service is instantiated through the `NinjectServiceProvider` an additional `ServiceProviderScopeParameter` is added to each request by means of the `ServiceProviderScopeResolutionRoot`. This parameter allows this custom handling to kick in.

This means that if you resolve a service that was registered through the `IServiceCollection` through the `IServiceProvider`, the behavior will be different from the same service instantiated directly through the kernel. So, while it is certainly possible to "cross-instantiate" services, it is usually not advisable.

### Disposal Ordering
Microsoft DI makes some very strong assumptions about the disposal order of services. Generally, it assumes that all services that are disposed are disposed in the exact reverse order in which they were instantiated. Additionally, it assumes that transient services are disposed when their associated `IServiceScope` is disposed.

This is implemented by replacing the Ninject default `DisposableStrategy` with a `OrderedDisposalStrategy` that tracks _all_ created instances (using weak references) and makes sure that disposal is somewhat grouped when an `IServiceScope` is disposed and that all services that have gone out of scope are disposed in the right order. The details here are a bit complicated, so please check the source code if you need to know more.

### Binding Precedence
This is one of the _weirdest_ behaviors of Microsoft DI. It is possible to register multiple descriptors for the exact same service type and still resolve a _single_ service instance. In that situation, it will use the descriptor that was registered **last**.

This is another case where instantiation throught the `IServiceProvider` will behave differently from instantiating the service through the kernel. When done through the kernel, this will of course throw an exception because it is unable to resolve a unique binding. To "fix" this for requests through the service provider, we replaced the Ninject default `BindingPrecedenceComparer` with `IndexedBindingPrecedenceComparer` that takes into account the _index_ of the binding which is added when building the service provider.

### Generics with Constraints
In ASP.NET Core, there are some scenarios with multi-injection where an open generic interface is bound to various different (open) implementations where _some_ of the implementations, while implementing the same interface, have additional binding constraints that the interface does not have. Microsoft DI makes sure that when an actual service is requested which of course has to define/close the generic type only those bindings are considered for which the generic type constraints actually work.

For example:
```
interface IMyInterface<T> { ... }
interface MyImplOne<T> { ... }
interface MyImplTwo<T> where T : IFoo { ... }

* IMyInterface<> is bound to MyImplOne<>
* IMyInterface<> is bound to MyImplTwo<>

1. Getting all instances of IMyInterface<Bar> where Bar does not implement IFoo
   => [MyImplOne<Bar>]
2. Getting all instances of IMyInterface<Foo> where Foo : IFoo
   => [MyImplOne<Foo>, MyImplTwo<Foo>]
```

This is comparatively easy to fix by replacing the Ninject default `OpenGenericBindingResolver` with a slightly improved `ConstrainedGenericBindingResolver`. This one creates the same result when resolved through the Ninject kernel, but **only** if the service was registered through the `IServiceCollection` because that is the only way to add the additional implementation class metadata that is needed for this check.


# Versioning
The package version numbers are chosen to align with the version of ASP.NET Core they were built against.

| Version | ASP.NET Core Version | Ninject Version | Target Frameworks              | Notes                                                                                                                   |
|---------|----------------------|-----------------|--------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| 9.*     | 9.0                  | 3.3.4           | net9.0, net8.0                 | The current _mainline_ version for use together with ASP.NET Core 9 and 8                                               |
| 8.*     | 8.0                  | 3.3.4           | net8.0, net7.0, net6.0, net5.0 | "v-prev" with support for ASP.NET Core 8, 7, 6 or 5                                                                     |
| 5.*     | 5.0                  | 3.3.4           | net5.0                         | Old NET 5 version. Obsolete now and included in 7.* line                                                                |
| 3.0.*   | 3.0.*, 3.1.*         | 3.3.4           | netcoreapp3.0, netcoreapp3.1   | The last .NET Core version. No longer maintained.                                                                       |
| 2.2.*   | 2.2.*                | 3.3.4           | netstandard2.0, netcoreapp2.2  | Should only be used as a "transitional" version when migrating to more recent .NET Core versions. No longer maintained. |
