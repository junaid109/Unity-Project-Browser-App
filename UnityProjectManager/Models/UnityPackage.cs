using CommunityToolkit.Mvvm.ComponentModel;

namespace UnityProjectManager.Models
{
    public partial class UnityPackage : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _version;

        public UnityPackage(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}
