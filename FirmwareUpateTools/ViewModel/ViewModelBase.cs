using System;
using System.ComponentModel;

namespace FirmwareUpateTools.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #region IDisposable Members

        public void Dispose()
        {
            this.OnDispose();
        }

        protected virtual void OnDispose()
        {
        }

        #endregion
    }
}
