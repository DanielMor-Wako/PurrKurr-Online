using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers
{
    public sealed class HandlersCoordinator : IDisposable
    {
        private static readonly Dictionary<IHandler, Type> _handlers = new();

        public HandlersCoordinator()
        {
        }

        /// <summary>
        /// Add Handlers individually or as a collection
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
        /// Get the handler from the Dictionary by class type that implements IHandler
        /// Return null for reference types or default value for value types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetHandler<T>() where T : class, IHandler
        {
            return _handlers.Keys
                .OfType<T>()
                .FirstOrDefault();
        }
        public T GetHandlerLinq<T>() where T : class, IHandler
        {
            return _handlers.Keys.Where((x) => x.GetType() == typeof(T)) as T;
        }

        /// <summary>
        /// Updates all the handlers that require a frequent update loop
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
        public void AddBindableHandlers(IEnumerable<IBindableHandler> _bindableHandlers)
        {
            UnbindHandlers();

            AddHandlers(_bindableHandlers);

            foreach (var handler in _handlers.Keys.OfType<IBindableHandler>())
            {
                handler.Bind();
            }

        }

        /// <summary>
        /// Unbind Handlers
        /// </summary>
        private void UnbindHandlers()
        {
            foreach (var handler in _handlers.Keys.OfType<IBindableHandler>())
            {
                handler?.Unbind();
            }
        }

        /// <summary>
        /// Clean the bindable Handlers
        /// </summary>
        private void DisposeHandlers()
        {
            foreach (var handler in _handlers.Keys)
            {
                handler?.Dispose();
            }
        }

        /// <summary>
        /// Clean up
        /// </summary>
        public void Dispose()
        {
            UnbindHandlers();
            DisposeHandlers();
            _handlers.Clear();
        }
    }
}