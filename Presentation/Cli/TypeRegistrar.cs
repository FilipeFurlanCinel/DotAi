namespace Presentation;

using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;

    public TypeRegistrar(IServiceCollection builder)
    {
        _builder = builder;
    }

    public void Register(Type service, Type implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _builder.AddSingleton(service, _ => factory());
    }

    public ITypeResolver Build()
    {
        var provider = _builder.BuildServiceProvider();
        return new TypeResolver(provider);
    }
}

public sealed class TypeResolver : ITypeResolver, IDisposable
{
    private readonly ServiceProvider _root;
    private readonly IServiceScope _scope;

    public TypeResolver(ServiceProvider provider)
    {
        _root = provider ?? throw new ArgumentNullException(nameof(provider));
        _scope = _root.CreateScope();
    }

    public object? Resolve(Type? type)
    {
        if (type is null)
            return null;

        return _scope.ServiceProvider.GetService(type);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _root.Dispose();
    }
}
