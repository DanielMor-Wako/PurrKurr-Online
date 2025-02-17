using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.StrategyPatterns
{

    public abstract class FactoryPattern { }

    /// <summary>
    /// Generic interface for all factories.
    /// </summary>
    public interface IFactory<T> where T : class
    {
        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        T Create(Type type);
    }

    /// <summary>
    /// Abstract base class for factories, providing a default implementation for object creation.
    /// </summary>
    public abstract class FactoryPattern<T> : IFactory<T> where T : class
    {
        /// <summary>
        /// Creates and returns an instance of the specified type
        /// </summary>
        public virtual T Create(Type type)
        {
            if (type == null)
            {
                Debug.LogError("Type cannot be null.");
                return default;
            }

            if (!typeof(T).IsAssignableFrom(type))
            {
                Debug.LogError($"Type {type.Name} does not implement {typeof(T).Name}.");
                return default;
            }

            try
            {
                return (T)Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create instance of type {type.Name}: {ex.Message}");
                return default;
            }
        }
    }
}