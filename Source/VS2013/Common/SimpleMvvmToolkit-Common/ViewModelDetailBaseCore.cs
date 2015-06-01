﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

// Bring model extensions into scope
using SimpleMvvmToolkit.ModelExtensions;
using System.Linq.Expressions;

// Alias for property dictionary
using PropertyDictionary = System.Collections.Generic.Dictionary<string, System.ComponentModel.PropertyChangedEventHandler>;

namespace SimpleMvvmToolkit
{
    /// <summary>
    /// Base class for detail view-models. Also provides support for IEditableDataObject.
    /// </summary>
    /// <typeparam name="TViewModel">Class inheriting from ViewModelBase</typeparam>
    /// <typeparam name="TModel">Detail entity type</typeparam>
    public abstract class ViewModelDetailBaseCore<TViewModel, TModel>
        : ViewModelBaseCore<TViewModel>, IEditableObject
        where TModel : class, INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dispatcher">Dispatcher for cross-thread operations.</param>
        /// <param name="messageBus">MessageBus for communication among view models.</param>
        protected ViewModelDetailBaseCore(IDispatcher dispatcher, MessageBusCore messageBus) 
            : base(dispatcher, messageBus)
        {
        }

        /// <summary>
        /// Model meta-properties which should be ignored when handling property changed events,
        /// and when dirty-checking or performing validation.
        /// </summary>
        protected virtual List<string> ModelMetaProperties
        {
            get { return _modelMetaProperties; }
            set { _modelMetaProperties = value; }
        }
        private List<string> _modelMetaProperties = new List<string>
        {
            "HasErrors", "EntityConflict", "ValidationErrors", "HasValidationErrors",
            "EntityState", "HasChanges", "IsReadOnly", "EntityActions"
        };

        /// <summary>
        /// Detail entity
        /// </summary>
        public virtual TModel Model
        {
            get { return modelField; }
            set
            {
                modelField = value;
                BindingHelper.InternalNotifyPropertyChanged("Model",
                    this, propertyChangedField, Dispatcher);
            }
        }

        /// <summary>
        /// Data entity accessible to derived classes.
        /// </summary>
        protected TModel modelField;

        // Handler for associate properties
        private readonly Dictionary<string, PropertyDictionary> _assocPropsHandlers
            = new Dictionary<string, PropertyDictionary>();

        /// <summary>
        /// Propagates changes from model property to view-model property.
        /// </summary>
        /// <typeparam name="TModelResult">Model property type</typeparam>
        /// <typeparam name="TViewModelResult">View-model property type</typeparam>
        /// <param name="modelProperty">Model property</param>
        /// <param name="viewModelProperty">View-model property</param>
        protected virtual void AssociateProperties<TModelResult, TViewModelResult>
            (Expression<Func<TModel, TModelResult>> modelProperty,
             Expression<Func<TViewModel, TViewModelResult>> viewModelProperty)
        {
            // Convert expressions to a property names
            string modelPropertyName = ((MemberExpression)modelProperty.Body).Member.Name;
            string viewModelPropertyName = ((MemberExpression)viewModelProperty.Body).Member.Name;

            // Get handlers
            if (!_assocPropsHandlers.ContainsKey(modelPropertyName))
                _assocPropsHandlers.Add(modelPropertyName, new PropertyDictionary());
            var handlers = _assocPropsHandlers[modelPropertyName];

            // Propagate model to view-model property change
            PropertyChangedEventHandler handler = (s, ea) =>
            {
                if (ea.PropertyName == modelPropertyName)
                {
                    BindingHelper.InternalNotifyPropertyChanged(viewModelPropertyName,
                        this, propertyChangedField, Dispatcher);
                }
            };

            // Add handler to property changed event
            Model.PropertyChanged += handler;

            // Add handler to handlers list
            handlers.Add(viewModelPropertyName, handler);
        }

        /// <summary>
        /// Unsubscribe from changes to model property.
        /// </summary>
        /// <typeparam name="TModelResult">Model property type</typeparam>
        /// <typeparam name="TViewModelResult">View-model property type</typeparam>
        /// <param name="modelProperty">Model property</param>
        /// <param name="viewModelProperty">View-model property</param>
        protected virtual void UnAssociateProperties<TModelResult, TViewModelResult>
            (Expression<Func<TModel, TModelResult>> modelProperty,
             Expression<Func<TViewModel, TViewModelResult>> viewModelProperty)
        {
            // Convert expressions to a property names
            string modelPropertyName = ((MemberExpression)modelProperty.Body).Member.Name;
            string viewModelPropertyName = ((MemberExpression)viewModelProperty.Body).Member.Name;

            // Get handlers
            PropertyDictionary handlers;
            if (_assocPropsHandlers.TryGetValue(modelPropertyName, out handlers))
            {
                // Get handler
                PropertyChangedEventHandler handler;
                if (handlers.TryGetValue(viewModelPropertyName, out handler))
                {
                    // Remove handler from property changed event
                    Model.PropertyChanged -= handler;

                    // Remove handlers from handlers list
                    handlers.Remove(viewModelPropertyName);

                    // Remove key if no handlers
                    if (handlers.Count == 0) _assocPropsHandlers.Remove(modelPropertyName);
                }

            }
        }

        private TModel Original { get; set; }

        private TModel Copy
        {
            get { return _copy; }
            set
            {
                // Fire IsDirty property changed when a model property changes
                PropertyChangedEventHandler handler = (s, ea) =>
                {
                    if (!_modelMetaProperties.Contains(ea.PropertyName))
                    {
                        BindingHelper.InternalNotifyPropertyChanged
                            ("IsDirty", this, propertyChangedField, Dispatcher);
                    }
                };

                // BeginEdit called
                if (value != null)
                {
                    value.PropertyChanged += handler;
                }
                // EditEdit or CancelEdit called
                else if (_copy != null)
                {
                    _copy.PropertyChanged -= handler;
                }
                _copy = value;
            }
        }
        private TModel _copy;

        /// <summary>
        /// Caches a deep clone of the entity
        /// </summary>
        public void BeginEdit()
        {
            // Throw an exception if Entity not supplied
            if (Model == null)
            {
                throw new InvalidOperationException("Entity must be set");
            }

            // Return if we're already editing
            if (Copy != null) return;

            // Copy entity
            Original = Model;
            Copy = Model.Clone();

            // Point entity to the copy
            Model = Copy;

            // Notify IsEditing, IsDirty
            BindingHelper.InternalNotifyPropertyChanged("IsEditing",
                    this, propertyChangedField, Dispatcher);
            BindingHelper.InternalNotifyPropertyChanged("IsDirty",
                    this, propertyChangedField, Dispatcher);

            // Post-processing
            OnBeginEdit();
        }

        /// <summary>
        /// Restores original
        /// </summary>
        public void CancelEdit()
        {
            // Return if BeginEdit not called first
            if (Copy == null) return;

            // Point entity to original
            Model = Original;

            // Clear copy
            Copy = null;

            // Notify IsEditing, IsDirty
            BindingHelper.InternalNotifyPropertyChanged("IsEditing",
                    this, propertyChangedField, Dispatcher);
            BindingHelper.InternalNotifyPropertyChanged("IsDirty",
                    this, propertyChangedField, Dispatcher);

            // Post-processing
            OnCancelEdit();
        }

        /// <summary>
        /// Copies property values from clone to original.
        /// </summary>
        public void EndEdit()
        {
            // Return if BeginEdit not called first
            if (Copy == null) return;

            // Tranfer values from copy to original
            Copy.CopyValuesTo(Original);

            // Point entity to original
            Model = Original;

            // Clear copy
            Copy = null;

            // Notify IsEditing, IsDirty
            BindingHelper.InternalNotifyPropertyChanged("IsEditing",
                    this, propertyChangedField, Dispatcher);
            BindingHelper.InternalNotifyPropertyChanged("IsDirty",
                    this, propertyChangedField, Dispatcher);

            // Post-processing
            OnEndEdit();
        }

        /// <summary>
        /// Model has executed BeginEdit.
        /// </summary>
        protected virtual void OnBeginEdit() { }

        /// <summary>
        /// Model has executed EndEdit.
        /// </summary>
        protected virtual void OnEndEdit() { }

        /// <summary>
        /// Model has executed CancelEdit
        /// </summary>
        protected virtual void OnCancelEdit() { }

        /// <summary>
        /// BeginEdit has been called; EndEdit or CancelEdit has not been called.
        /// </summary>
        public bool IsEditing
        {
            get { return Copy != null; }
        }

        /// <summary>
        /// Entity has been changed while editing.
        /// </summary>
        public bool IsDirty
        {
            get
            {
                // BeginEdit has been called
                if (Copy != null && Original != null)
                {
                    bool areSame = Copy.AreSame(Original, _modelMetaProperties.ToArray());
                    return !areSame;
                }
                return false;
            }
        }
    }
}
