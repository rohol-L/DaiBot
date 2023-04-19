using Autofac;
using DaiBot.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Core.Utils
{
    public static class MyInject
    {
        public static T? CreateInstance<T>(params ILifetimeScope[] scopes)
        {
            return (T?)CreateInstance(typeof(T), scopes);
        }

        public static object? CreateInstance(Type t, ILifetimeScope scope, List<IService>? inject)
        {
            var cis = t.GetConstructors();
            if (cis.Length == 0)
            {
                return Activator.CreateInstance(t);
            }
            var ci = cis.Length == 1 ? cis[0] : cis.OrderByDescending(c => c.GetParameters().Length).First();
            var pi = ci.GetParameters();
            var param = new object[pi.Length];
            for (int i = 0; i < pi.Length; i++)
            {
                Type targetType = pi[i].ParameterType;
                if (inject != null)
                {
                    foreach (var item in inject)
                    {
                        if (targetType == item.GetType())
                        {
                            param[i] = item;
                            break;
                        }
                    }
                }
                if (param[i] == null)
                {
                    param[i] = scope.Resolve(pi[i].ParameterType);
                }
            }
            return ci.Invoke(param);
        }

        public static object? CreateInstance(Type t, params ILifetimeScope[] scopes)
        {
            var cis = t.GetConstructors();
            if (cis.Length == 0)
            {
                return Activator.CreateInstance(t);
            }
            var ci = cis.Length == 1 ? cis[0] : cis.OrderByDescending(c => c.GetParameters().Length).First();
            var pi = ci.GetParameters();
            var param = new object[pi.Length];
            for (int i = 0; i < pi.Length; i++)
            {
                Type targetType = pi[i].ParameterType;
                foreach (var scope in scopes)
                {
                    if (scope.TryResolve(targetType, out var obj))
                    {
                        param[i] = obj;
                    }
                }
                if (param[i] == null)
                {
                    throw new Exception("未注册类型：" + targetType.FullName);
                }
            }
            return ci.Invoke(param);
        }
    }
}
