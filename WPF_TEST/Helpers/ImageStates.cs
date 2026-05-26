using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;

namespace WPF_TEST.Helpers
{
    public class ImageStates : ObservableObject
    {
        private int _progress;
        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isLoaded;
        public bool IsLoaded
        {
            get => _isLoaded;
            set => SetProperty(ref _isLoaded, value);
        }
        
        private string _imageURL = "";
        public string ImageURL
        {
            get => _imageURL;
            set => SetProperty(ref _imageURL, value);
        }

        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public CancellationTokenSource? CancellationTokenSource { get; set; }
    }
}
