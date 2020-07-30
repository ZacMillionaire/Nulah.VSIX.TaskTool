using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulah.VSIX.TaskTool.StandardLib.Models
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Static to persist across all view models using <see cref="ViewModelBase"/>
        /// </summary>
        private static Dictionary<Type, object> _viewDependencies;

        /// <summary>
        /// Registers a view dependency to all view models that inherit from <see cref="ViewModelBase"/>
        /// <para>Only one unique type can be registered at a time</para>
        /// </summary>
        /// <typeparam name="TDependency"></typeparam>
        /// <param name="dependencyObject"></param>
        public void RegisterDependency<TDependency>(TDependency dependencyObject)
        {
            // Initialise the view dependency dictionary if this is the first registration of a type
            if (_viewDependencies == null)
            {
                _viewDependencies = new Dictionary<Type, object>(1);
            }

            var dependencyType = typeof(TDependency);

            // If we don't have the type, register it
            if (_viewDependencies.ContainsKey(dependencyType) == false)
            {
                _viewDependencies.Add(dependencyType, dependencyObject);
            }
            else
            {
                // If we do, but we're getting a different reference, throw an exception, otherwise we're attempting to readd the same reference
                // We only want 1 type of a dependency to be added
                if (ReferenceEquals(_viewDependencies[dependencyType], dependencyObject) == false)
                {
                    throw new Exception("Unable to assign dependency: a different reference of the same type has already been added");
                }
            }
        }

        /// <summary>
        /// Registers a view dependency to all view models that inherit from <see cref="ViewModelBase"/>
        /// <para>Only one unique type can be registered at a time</para>
        /// </summary>
        /// <typeparam name="TDependency"></typeparam>
        public void RegisterDependency<TDependency>() where TDependency : new()
        {
            var o = Activator.CreateInstance<TDependency>();
            RegisterDependency(o);
        }

        /// <summary>
        /// Returns the registered view dependency, if previously registered.
        /// <para>Returns the default value for <typeparamref name="TDependency"/> if not found, so values should be checked for null.</para>
        /// </summary>
        /// <typeparam name="TDependency"></typeparam>
        /// <returns></returns>
        public TDependency GetDependency<TDependency>()
        {
            // If the dependency dictionary hasn't been initalised, return the default of whatever type would be expected - which in most cases will be null
            if (_viewDependencies == null)
            {
                return default;
            }

            if (_viewDependencies.ContainsKey(typeof(TDependency)) == true)
            {
                return (TDependency)_viewDependencies[typeof(TDependency)];
            }

            // Return default (null for most types) if the dependency isn't found
            return default;
        }

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (ThrowOnInvalidPropertyName)
                {
                    throw new Exception(msg);
                }
                else
                {
                    Debug.Fail(msg);
                }
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            OnDispose();
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose()
        {
        }
    }
}
