﻿using System;
using System.Windows;
using System.Threading;
using System.Collections.ObjectModel;

// Toolkit namespace
using SimpleMvvmToolkit;

namespace $rootnamespace$
{
    /// <summary>
    /// This class extends ViewModelDetailBase which implements IEditableDataObject.
    /// <para>
    /// Specify type being edited <strong>DetailType</strong> as the second type argument
    /// and as a parameter to the seccond ctor.
    /// </para>
    /// <para>
    /// Use the <strong>mvvmprop</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// </summary>
    public class $safeitemname$ : ViewModelDetailBase<$safeitemname$, /* DetailType */>
    {
        // TODO: Add a member for IXxxServiceAgent
        private /* IXxxServiceAgent */ serviceAgent;

        // Default ctor
        public $safeitemname$() { }

        // TODO: Ctor to set base.Model to DetailType
        public $safeitemname$(/* DetailType */ model)
        {
            base.Model = model;
        }

        // TODO: ctor that accepts IXxxServiceAgent
        public $safeitemname$(/* IXxxServiceAgent */ serviceAgent)
        {
            this.serviceAgent = serviceAgent;
        }

        // TODO: Add events to notify the view or obtain data from the view
        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;

        // TODO: Add properties using the mvvmprop code snippet

        // TODO: Add methods that will be called by the view

        // TODO: Optionally add callback methods for async calls to the service agent
        
        // Helper method to notify View of an error
        private void NotifyError(string message, Exception error)
        {
            // Notify view of an error
            Notify(ErrorNotice, new NotificationEventArgs<Exception>(message, error));
        }
    }
}