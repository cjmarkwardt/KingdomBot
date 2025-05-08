namespace Markwardt;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public class DefaultImplementationAttribute(Type implementation) : Attribute
{
    public Type Implementation => implementation;
}

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
public class DefaultImplementationAttribute<T>() : DefaultImplementationAttribute(typeof(T));