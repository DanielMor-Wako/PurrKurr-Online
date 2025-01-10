using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers
{
    public sealed class HandlersCoordinator : IDisposable
    {
        private Dictionary<IHandler, Type> _handlers;

        public HandlersCoordinator()
        {
            _handlers = new Dictionary<IHandler, Type>();
        }

        /// <summary>
        /// Add Handlers individually or as collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handlers"></param>
        public void AddHandlers<T>(IEnumerable<T> handlers) where T : class, IHandler
        {
            foreach (var handler in handlers)
            {
                AddHandler(handler);
            }
        }

        public void AddHandler<T>(T handler) where T : class, IHandler
        {
            _handlers[handler] = typeof(T);
        }

        /// <summary>
        /// Get the handler from the Dictionary By class type that implements IHandler
        /// Return null for reference types or default value for value types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetHandler<T>() where T : class, IHandler
        {
            var typeToMatch = typeof(T);
            foreach (var kvp in _handlers)
            {
                if (kvp.Key.GetType() == typeToMatch)
                {
                    return kvp.Key as T;
                }
            }
            return default;
        }

        /// <summary>
        /// Updates all the handlers that require frequent update loop
        /// </summary>
        public void UpdateHandlers()
        {
            foreach (var handler in _handlers.Keys.OfType<IUpdateProcessHandler>())
            {
                handler.UpdateProcess(Time.deltaTime);
            }
        }

        /// <summary>
        /// Set new Bindable Handlers, also unbind previous handlers
        /// </summary>
        /// <param name="_bindableHandlers"></param>
        public void BindHandlers(IEnumerable<IBindableHandler> _bindableHandlers)
        {
            UnbindHandlers();

            foreach (var handler in _bindableHandlers)
            {
                AddHandler(handler);

                handler.Bind();
            }
        }

        /// <summary>
        /// Clean the bindable Handlers
        /// </summary>
        private void UnbindHandlers()
        {
            foreach (var handler in _handlers.Values.OfType<IBindableHandler>())
            {
                handler?.Unbind();
            }
        }

        /// <summary>
        /// Clean up
        /// </summary>
        public void Dispose()
        {
            UnbindHandlers();
            
            _handlers = null;
        }
    }
}