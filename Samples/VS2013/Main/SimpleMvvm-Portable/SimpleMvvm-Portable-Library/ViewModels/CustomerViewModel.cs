﻿using System;
using System.Windows.Input;

// Toolkit namespace
using SimpleMvvmToolkit;

// Toolkit extension methods
using SimpleMvvmToolkit.ModelExtensions;

namespace SimpleMvvm_Portable_Library
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
    public class CustomerViewModel : ViewModelDetailBase<CustomerViewModel, Customer>
    {
        #region Initialization and Cleanup

        // Add a member for ICustomerServiceAgent
        private readonly ICustomerServiceAgent _serviceAgent;

        // Default ctor
        public CustomerViewModel() { }

        // Ctor that accepts ICustomerServiceAgent
        public CustomerViewModel(ICustomerServiceAgent serviceAgent)
        {
            _serviceAgent = serviceAgent;
        }

        #endregion

        #region Notifications

        // TODO: Add events to notify the view or obtain data from the view
        public event EventHandler<NotificationEventArgs<Exception>> ErrorNotice;

        #endregion

        #region Properties

        private string _bannerText = "Hello Simple MVVM Toolkit";
        public string BannerText
        {
            get
            {
                return _bannerText;
            }
            set
            {
                _bannerText = value;
                NotifyPropertyChanged(m => m.BannerText);
            }
        }

        #endregion

        #region Methods

        // Set the model to a new customer
        public void NewCustomer()
        {
            base.Model = _serviceAgent.CreateCustomer();
        }

        #endregion

        #region Commands

        public ICommand NewCustomerCommand
        {
            get
            {
                return new DelegateCommand(NewCustomer);
            }
        }

        #endregion

        #region Completion Callbacks

        // TODO: Optionally add callback methods for async calls to the service agent

        #endregion

        #region Helpers

        // Helper method to notify View of an error
        private void NotifyError(string message, Exception error)
        {
            // Notify view of an error
            Notify(ErrorNotice, new NotificationEventArgs<Exception>(message, error));
        }

        #endregion
    }
}