using Autofac;
using Autofac.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Core.Application
{
    public class DaiBotServices
    {
        readonly internal ContainerBuilder containerBuilder = new();

        internal DaiBotServices() { }

        public void AddSingleton(object instance)
        {
            containerBuilder.RegisterInstance(instance).AsSelf().SingleInstance();
        }

        public void AddSingleton<T>(object instance) where T : class
        {
            containerBuilder.RegisterInstance(instance).As<T>().SingleInstance();
        }

        public void AddSingleton<T>() where T : class
        {
            containerBuilder.RegisterType<T>().AsSelf().SingleInstance();
        }

        /// <summary>
        /// RegisterType T as K
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        public void AddSingleton<T, K>() where T : class where K : class
        {
            containerBuilder.RegisterType<T>().As<K>().SingleInstance();
        }

        public void AddScoped(object instance)
        {
            containerBuilder.RegisterInstance(instance).AsSelf().InstancePerLifetimeScope();
        }

        public void AddScoped<T>(object instance) where T : class
        {
            containerBuilder.RegisterInstance(instance).As<T>().InstancePerLifetimeScope();
        }

        public void AddScoped<T>() where T : class
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerLifetimeScope();
        }

        /// <summary>
        /// RegisterType T as K
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        public void AddScoped<T, K>() where T : class where K : class
        {
            containerBuilder.RegisterType<T>().As<K>().InstancePerLifetimeScope();
        }

        public void AddTransient(object instance)
        {
            containerBuilder.RegisterInstance(instance).AsSelf().InstancePerDependency();
        }

        public void AddTransient<T>(object instance) where T : class
        {
            containerBuilder.RegisterInstance(instance).As<T>().InstancePerDependency();
        }

        public void AddTransient<T>() where T : class
        {
            containerBuilder.RegisterType<T>().AsSelf().InstancePerDependency();
        }

        /// <summary>
        /// RegisterType T as K
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        public void AddTransient<T, K>() where T : class where K : class
        {
            containerBuilder.RegisterType<T>().As<K>().InstancePerDependency();
        }

        public IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterType<T>() where T : class
        {
            return containerBuilder.RegisterType<T>();
        }


        internal IContainer Build()
        {
            return containerBuilder.Build();
        }
    }
}
