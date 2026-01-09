using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnityProjectManager.Views
{
    public partial class ProjectCardView : UserControl
    {
        public ProjectCardView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
