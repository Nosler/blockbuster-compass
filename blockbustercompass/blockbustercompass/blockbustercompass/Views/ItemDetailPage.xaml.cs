using System.ComponentModel;
using Xamarin.Forms;
using blockbustercompass.ViewModels;

namespace blockbustercompass.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}
