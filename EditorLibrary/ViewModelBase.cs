using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace EditorLibrary {

    public abstract class ViewModelBase : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        public event CollectionChangeEventHandler CollectionChanged;


        public void SetValue<T>(ref T property, T value, [CallerMemberName] string propertyName = null) {

            if (property != null) {

                if (property.Equals(value)) {

                    return;
                }

                OnPropertyChanged(propertyName);
                property = value;
            }
        }

        public abstract void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e);



        protected virtual void OnCollectionChanged(object sender, CollectionChangeEventArgs e) {

            this.CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName) {

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
