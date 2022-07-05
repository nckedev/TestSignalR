using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Client;

namespace Shared;

public static class HubExtensions
{
    /// <summary>
    /// Receive method for all messages from Agent to hub
    /// </summary>
    public static async Task SendToHub<T>(this HubConnection hub, Guid requestId, T obj)
    {
        await hub.SendAsync("Receive", requestId, obj);
    }

    public static IDisposable OnAgent<T1>(this HubConnection connection,
        Expression<Func<IAgentMethods, Func<T1, Task>>> method,
        Func<T1, Task> onAgent) =>
        connection.BindOnInterface(method, onAgent);

    public static IDisposable OnAgent<T1, T2>(this HubConnection connection,
        Expression<Func<IAgentMethods, Func<T1, T2, Task>>> method,
        Func<T1, T2, Task> onAgent) =>
        connection.BindOnInterface(method, onAgent);

    public static IDisposable OnAgent<T1, T2, T3>(this HubConnection connection,
        Expression<Func<IAgentMethods, Func<T1, T2, T3, Task>>> method,
        Func<T1, T2, T3, Task> onAgent) =>
        connection.BindOnInterface(method, onAgent);

    private static IDisposable BindOnInterface<T>(this HubConnection connection,
        Expression<Func<IAgentMethods, Func<T, Task>>> boundMethod, Func<T, Task> handler)
        => connection.On(_GetMethodName(boundMethod), handler);

    private static IDisposable BindOnInterface<T1, T2>(this HubConnection connection,
        Expression<Func<IAgentMethods, Func<T1, T2, Task>>> boundMethod, Func<T1, T2, Task> handler)
        => connection.On(_GetMethodName(boundMethod), handler);

    private static IDisposable BindOnInterface<T1, T2, T3>(this HubConnection connection,
        Expression<Func<IAgentMethods, Func<T1, T2, T3, Task>>> boundMethod,
        Func<T1, T2, T3, Task> handler)
        => connection.On(_GetMethodName(boundMethod), handler);

    private static string _GetMethodName<T>(Expression<T> boundMethod)
    {
        var unaryExpression = (UnaryExpression) boundMethod.Body;
        var methodCallExpression = (MethodCallExpression) unaryExpression.Operand;
        var methodInfoExpression = (ConstantExpression?) methodCallExpression.Object;
        var methodInfo = (MethodInfo?) methodInfoExpression?.Value;
        return methodInfo?.Name ?? throw new Exception("methodInfo i _GetMethodName är null");
    }

    private static string _GetExpressionName<T>(Expression<Func<IAppClientMethods, Task<T>>> exp)
    {
        var a = ((MethodCallExpression) exp.Body).Method.Name;
        return a;
    }

    private static List<object> _GetArguments<T>(Expression<Func<IAppClientMethods, Task<T>>> invokeMethod)
    {
        var list = new List<object>();

        if (invokeMethod.Body is MethodCallExpression methodCallExpression)
        {
            var b = methodCallExpression.Arguments;
            foreach (var arg in b)
            {
                if (arg is ConstantExpression constantExpression && constantExpression.Value != null)
                {
                    list.Add(constantExpression.Value);
                }
                else if (arg is BinaryExpression binaryExpression)
                {
                    var del = Expression.Lambda(binaryExpression, invokeMethod.Parameters).Compile();
                    var o = del.DynamicInvoke(binaryExpression.Left);
                    list.Add(o ?? throw new ArgumentException("BinaryExpression evaluated to null"));
                }
                else if (arg is MemberExpression memberExpression)
                {
                    var objectMember = Expression.Convert(memberExpression, typeof(object));
                    var getterlambda = Expression.Lambda<Func<object>>(objectMember);
                    var getValue = getterlambda.Compile();
                    list.Add(getValue());
                }
                else if (arg is MethodCallExpression innerMethodCallExpression)
                {
                    var res = Expression.Lambda(innerMethodCallExpression, null).Compile().DynamicInvoke();
                    list.Add(res ?? throw new ArgumentException("MethodCallExpression evaluated to null"));
                }
                else
                {
                    throw new Exception("Unknown expression type");
                }
            }
        }

        return list;
    }


    /// <summary>
    /// invokes IAppClientMethod on hub<br />
    /// From App to Hub<br />
    /// <code> TRetrun var = await hub.InvokeOnHub(x => x.IAppClientMethod("123")); </code>
    /// </summary>
    public static async Task<TReturn> InvokeOnHub<TReturn>(this HubConnection connection,
        Expression<Func<IAppClientMethods, Task<TReturn>>> f, CancellationToken ct = default)
    {
        var args = _GetArguments(f);
        return args.Count switch
        {
            0 => await connection.InvokeAsync<TReturn>(_GetExpressionName(f), cancellationToken: ct),
            1 => await connection.InvokeAsync<TReturn>(_GetExpressionName(f), args[0], cancellationToken: ct),
            2 => await connection.InvokeAsync<TReturn>(_GetExpressionName(f), args[0], args[1], cancellationToken: ct),
            3 => await connection.InvokeAsync<TReturn>(_GetExpressionName(f), args[0], args[1], args[2],
                cancellationToken: ct),
            4 => await connection.InvokeAsync<TReturn>(_GetExpressionName(f), args[0], args[1], args[2], args[3],
                cancellationToken: ct),
            _ => throw new ArgumentException("kan inte hantera mer än 4 arguments")
        };
    }
}