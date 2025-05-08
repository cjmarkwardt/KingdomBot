namespace Markwardt;

public class Invoker(MethodBase method)
{
    private readonly Lazy<Invocation> target = new(() => new(method));

    public bool Invoke(object? instance, Func<ParameterInfo, object?> getArgument, out object? result)
        => target.Value.Invoke(instance, getArgument, out result);

    public object? Invoke(object? instance, Func<ParameterInfo, object?> getArgument)
        => Invoke(instance, getArgument, out object? result) ? result : throw new InvalidOperationException("No result");

    private sealed class Invocation
    {
        private delegate object? Target(object? instance, object?[]? arguments);

        public Invocation(MethodBase method)
        {
            parameters = method.GetParameters();
            arguments = new object?[parameters.Length];

            ParameterExpression instanceParameter = Expression.Parameter(typeof(object));
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object?[]));
            
            IEnumerable<Expression> argumentExpressions = parameters.Select((p, i) => Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(i)), p.ParameterType));

            Expression body;
            if (method is ConstructorInfo constructor)
            {
                hasResult = true;
                body = Expression.New(constructor, argumentExpressions);
            }
            else if (method is MethodInfo normalMethod)
            {
                hasResult = normalMethod.ReturnType != typeof(void);
                if (normalMethod.IsStatic)
                {
                    body = Expression.Call(normalMethod, argumentExpressions);
                }
                else
                {
                    body = Expression.Call(Expression.Convert(instanceParameter, normalMethod.DeclaringType.NotNull()), normalMethod, argumentExpressions);
                }

                if (normalMethod.ReturnType == typeof(void))
                {
                    body = Expression.Block(body, Expression.Constant(null));
                }
            }
            else
            {
                throw new InvalidOperationException();
            }

            target = Expression.Lambda<Target>(body, instanceParameter, argumentsParameter).Compile();
        }

        private readonly ParameterInfo[] parameters;
        private readonly object?[] arguments;
        private readonly bool hasResult;
        private readonly Target target;

        public bool Invoke(object? instance, Func<ParameterInfo, object?> getArgument, out object? result)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                arguments[i] = getArgument(parameters[i]);
            }

            result = target(instance, arguments);
            return hasResult;
        }
    }
}