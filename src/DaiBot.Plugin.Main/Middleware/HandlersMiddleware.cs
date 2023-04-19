using DaiBot.Core;
using DaiBot.Core.Application;
using System.Reflection;
using DaiBot.Core.FilterAttributes;
using Autofac;
using DaiBot.Core.Interface;

namespace DaiBot.Plugin.Main.Middleware
{
    public class HandlersMiddleware : IMiddleware
    {
        public int Order => int.MaxValue;

        public string Name => "处理程序中间件";

        readonly DaiBotPlugins plugins;

        public HandlersMiddleware(IApplication application)
        {
            plugins = application.Plugins;
        }

        public void Invoke(MessageContext context, Action next)
        {
            // todo: add policy
            bool enableDefaultHandler = true;
            var defaultHandles = new List<IHandlerBase>();

            using var scope = context.Scope;

            foreach (var handler in plugins.GetHandlers())
            {
                bool isDefault = false;
                bool checkedPass = true;
                Type handleType = handler.GetType();
                foreach (var filterAttribute in handleType.GetCustomAttributes<FilterAttribute>(true))
                {
                    if (filterAttribute is DefaultFilterAttribute)
                    {
                        isDefault = true;
                        continue;
                    }
                    if (filterAttribute is CustomerFilterAttribute && !InvokeCheck(handler, context, scope))
                    {
                        checkedPass = false;
                        break;
                    }
                    if (!InvokeCheck(filterAttribute, context, scope))
                    {
                        checkedPass = false;
                        break;
                    }
                }
                if (!checkedPass)
                {
                    continue;
                }
                if (isDefault)
                {
                    defaultHandles.Add(handler);
                    continue;
                }

                Console.WriteLine("handle:" + handler.GetType().Name);
                enableDefaultHandler = false;

                InvokeHandler(handler, context);
            }

            if (enableDefaultHandler)
            {
                foreach (var handler in defaultHandles)
                {
                    InvokeHandler(handler, context);
                }
            }

            next();
        }

        private static bool InvokeCheck(object checktarget, MessageContext context, ILifetimeScope scope)
        {
            Type targetType = checktarget.GetType();
            ParameterInfo[] parameterInfo = Array.Empty<ParameterInfo>();
            MethodInfo? methodInfo = null;
            foreach (var method in targetType.GetMethods())
            {
                if (method.Name == "Check" && method.DeclaringType == targetType)
                {
                    var pi = method.GetParameters();
                    if (parameterInfo.Length <= pi.Length)
                    {
                        parameterInfo = pi;
                        methodInfo = method;
                    }
                }
            }
            if (methodInfo == null)
            {
                return true;
            }
            object[] args = new object[parameterInfo.Length];
            for (int i = 0; i < parameterInfo.Length; i++)
            {
                var pi = parameterInfo[i];
                if (pi.ParameterType == typeof(MessageContext))
                {
                    args[i] = context;
                }
                else
                {
                    args[i] = scope.Resolve(pi.ParameterType);
                }
            }
            object? result = methodInfo.Invoke(checktarget, args);
            return Convert.ToBoolean(result);
        }

        private static void InvokeHandler(IHandlerBase handlerBase, MessageContext context)
        {
            if (handlerBase is IHandler handlerSync)
            {
                try
                {
                    var result = handlerSync.Handle(context);
                    if (result != null)
                    {
                        context.Callback?.Invoke(result.Rsps.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else if (handlerBase is IHandlerAsync handlerAsync)
            {
                handlerAsync.HandleAsync(context).ContinueWith(r =>
                {
                    if (r.Exception != null)
                    {
                        Console.WriteLine(r.Exception.Message);
                    }
                    else if (r.Result != null)
                    {
                        context.Callback?.Invoke(r.Result.Rsps.ToArray());
                    }
                });
            }
        }
    }
}
