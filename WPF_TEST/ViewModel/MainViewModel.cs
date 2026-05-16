using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WPF_TEST.Helpers;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Timers;
using System.Net.Http;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Threading;

namespace WPF_TEST.ViewModel
{
    public partial class MainViewModel : ObservableRecipient
    {
        private CancellationTokenSource _leftCancellationTokenSource;
        private CancellationTokenSource _centerCancellationTokenSource;
        private CancellationTokenSource _rightCancellationTokenSource;


        // ============== БЛОК СВОЙСТВ ==============
        private int _leftProgress;
        public int LeftProgress
        {
            get => _leftProgress;
            set => SetProperty(ref _leftProgress, value);
        }

        private int _centerProgress;
        public int CenterProgress
        {
            get => _centerProgress;
            set => SetProperty(ref _centerProgress, value);
        }

        private int _rightProgress;
        public int RightProgress
        {
            get => _rightProgress;
            set => SetProperty(ref _rightProgress, value);
        }

        private bool _isLeftLoading;
        public bool IsLeftLoading
        {
            get => _isLeftLoading;
            set
            {
                if (SetProperty(ref _isLeftLoading, value))
                {
                    LeftImageStopCommand?.NotifyCanExecuteChanged();
                    LeftImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isCenterLoading;
        public bool IsCenterLoading
        {
            get => _isCenterLoading;
            set
            {
                if (SetProperty(ref _isCenterLoading, value))
                {
                    CenterImageStopCommand?.NotifyCanExecuteChanged();
                    CenterImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isRightLoading;
        public bool IsRightLoading
        {
            get => _isRightLoading;
            set
            {
                if (SetProperty(ref _isRightLoading, value))
                {
                    RightImageStopCommand?.NotifyCanExecuteChanged();
                    RightImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isLeftLoaded;
        public bool IsLeftLoaded
        {
            get => _isLeftLoaded;
            set
            {
                if (SetProperty(ref _isLeftLoaded, value))
                {
                    LeftImageStopCommand?.NotifyCanExecuteChanged();
                    LeftImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isCenterLoaded;
        public bool IsCenterLoaded
        {
            get => _isCenterLoaded;
            set
            {
                if (SetProperty(ref _isCenterLoaded, value))
                {
                    CenterImageStopCommand?.NotifyCanExecuteChanged();
                    CenterImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _isRightLoaded;
        public bool IsRightLoaded
        {
            get => _isRightLoaded;
            set
            {
                if (SetProperty(ref _isRightLoaded, value))
                {
                    RightImageStopCommand?.NotifyCanExecuteChanged();
                    RightImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private double _totalProgress;
        public double TotalProgress
        {
            get => _totalProgress;
            set => SetProperty(ref _totalProgress, value);
        }

        
        private string _leftImageURL;
        public string LeftImageURL
        {
            get => _leftImageURL;
            set
            {
                if (SetProperty(ref _leftImageURL, value))
                {
                    LeftImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private string _centerImageURL;
        public string CenterImageURL
        {
            get => _centerImageURL;
            set
            {
                if (SetProperty(ref _centerImageURL, value))
                {
                    CenterImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private string _rightImageURL;
        public string RightImageURL
        {
            get => _rightImageURL;
            set
            {
                if (SetProperty(ref _rightImageURL, value))
                {
                    RightImageLoadCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        private BitmapImage _leftImageSource;
        public BitmapImage LeftImageSource
        {
            get => _leftImageSource;
            set => SetProperty(ref _leftImageSource, value);
        }

        private BitmapImage _centerImageSource;
        public BitmapImage CenterImageSource
        {
            get => _centerImageSource;
            set => SetProperty(ref _centerImageSource, value);
        }

        private BitmapImage _rightImageSource;
        public BitmapImage RightImageSource
        {
            get => _rightImageSource;
            set => SetProperty(ref _rightImageSource, value);
        }

        // ============== БЛОК КОМАНД ==============
        public IAsyncRelayCommand LeftImageLoadCommand { get; }
        public IAsyncRelayCommand CenterImageLoadCommand { get; }
        public IAsyncRelayCommand RightImageLoadCommand { get; }
        public IRelayCommand LeftImageStopCommand { get; }
        public IRelayCommand CenterImageStopCommand { get; }
        public IRelayCommand RightImageStopCommand { get; }
        public IAsyncRelayCommand LoadAllCommand { get; }

        public MainViewModel()
        {
            LeftImageLoadCommand = new AsyncRelayCommand(() => StartImageLoadAsync(ImageSide.Left), () => !IsLeftLoading && !string.IsNullOrWhiteSpace(LeftImageURL));
            CenterImageLoadCommand = new AsyncRelayCommand(() => StartImageLoadAsync(ImageSide.Center), () => !IsCenterLoading && !string.IsNullOrWhiteSpace(CenterImageURL));
            RightImageLoadCommand = new AsyncRelayCommand(() => StartImageLoadAsync(ImageSide.Right), () => !IsRightLoading && !string.IsNullOrWhiteSpace(RightImageURL));

            LeftImageStopCommand = new RelayCommand(() => StopImageLoad(ImageSide.Left), () => IsLeftLoading);
            CenterImageStopCommand = new RelayCommand(() => StopImageLoad(ImageSide.Center), () => IsCenterLoading);
            RightImageStopCommand = new RelayCommand(() => StopImageLoad(ImageSide.Right), () => IsRightLoading);

            LoadAllCommand = new AsyncRelayCommand(LoadAllImagesAsync);
        }


        private async Task StartImageLoadAsync(ImageSide side)
        {
            string url = side switch
            {
                ImageSide.Left => LeftImageURL,
                ImageSide.Center => CenterImageURL,
                ImageSide.Right => RightImageURL,
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Введите URL изображения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedImages");
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            string fileName = $"{side}_{DateTime.Now.Ticks}_{Guid.NewGuid():N}.jpg";
            string filePath = Path.Combine(imagesFolder, fileName);

            CancellationTokenSource cts = new CancellationTokenSource();

            switch (side)
            {
                case ImageSide.Left:
                    _leftCancellationTokenSource?.Cancel();
                    _leftCancellationTokenSource?.Dispose();
                    _leftCancellationTokenSource = cts;
                    IsLeftLoading = true;
                    LeftProgress = 0;
                    IsLeftLoaded = false;
                    break;
                case ImageSide.Center:
                    _centerCancellationTokenSource?.Cancel();
                    _centerCancellationTokenSource?.Dispose();
                    _centerCancellationTokenSource = cts;
                    IsCenterLoading = true;
                    CenterProgress = 0;
                    IsCenterLoaded = false;
                    break;
                case ImageSide.Right:
                    _rightCancellationTokenSource?.Cancel();
                    _rightCancellationTokenSource?.Dispose();
                    _rightCancellationTokenSource = cts;
                    IsRightLoading = true;
                    RightProgress = 0;
                    IsRightLoaded = false;
                    break;
            }

            try
            {
                BitmapImage bitmapImage = await DownloadImageAsync(url, filePath, cts.Token, side);

                if (!cts.Token.IsCancellationRequested && bitmapImage != null)
                {
                    switch (side)
                    {
                        case ImageSide.Left:
                            LeftImageSource = bitmapImage;
                            LeftProgress = 100;
                            IsLeftLoaded = true;
                            break;
                        case ImageSide.Center:
                            CenterImageSource = bitmapImage;
                            CenterProgress = 100;
                            IsCenterLoaded = true;
                            break;
                        case ImageSide.Right:
                            RightImageSource = bitmapImage;
                            RightProgress = 100;
                            IsRightLoaded = true;
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"Загрузка {side} отменена");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                switch (side)
                {
                    case ImageSide.Left:
                        IsLeftLoading = false;
                        break;
                    case ImageSide.Center:
                        IsCenterLoading = false;
                        break;
                    case ImageSide.Right:
                        IsRightLoading = false;
                        break;
                }

                UpdateTotalProgress();

                LeftImageLoadCommand.NotifyCanExecuteChanged();
                CenterImageLoadCommand.NotifyCanExecuteChanged();
                RightImageLoadCommand.NotifyCanExecuteChanged();
                LeftImageStopCommand.NotifyCanExecuteChanged();
                CenterImageStopCommand.NotifyCanExecuteChanged();
                RightImageStopCommand.NotifyCanExecuteChanged();
            }
        }

        private async Task<BitmapImage> DownloadImageAsync(string url, string filePath, CancellationToken cancellationToken, ImageSide side)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    long totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    long receivedBytes = 0;

                    using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            receivedBytes += bytesRead;

                            if (totalBytes > 0)
                            {
                                int progressPercentage = (int)((double)receivedBytes / totalBytes * 100);

                                switch (side)
                                {
                                    case ImageSide.Left:
                                        LeftProgress = progressPercentage;
                                        break;
                                    case ImageSide.Center:
                                        CenterProgress = progressPercentage;
                                        break;
                                    case ImageSide.Right:
                                        RightProgress = progressPercentage;
                                        break;
                                }

                                UpdateTotalProgress();
                            }
                        }
                    }
                }
            }

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(filePath);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        private void StopImageLoad(ImageSide side)
        {
            switch (side)
            {
                case ImageSide.Left:
                    _leftCancellationTokenSource?.Cancel();
                    IsLeftLoading = false;
                    LeftProgress = 0;
                    break;
                case ImageSide.Center:
                    _centerCancellationTokenSource?.Cancel();
                    IsCenterLoading = false;
                    CenterProgress = 0;
                    break;
                case ImageSide.Right:
                    _rightCancellationTokenSource?.Cancel();
                    IsRightLoading = false;
                    RightProgress = 0;
                    break;
            }

            UpdateTotalProgress();


        }

        private async Task LoadAllImagesAsync()
        {
            
            if (!(!string.IsNullOrWhiteSpace(LeftImageURL) || !string.IsNullOrWhiteSpace(CenterImageURL) || !string.IsNullOrWhiteSpace(RightImageURL)))
            {
                MessageBox.Show("Введите URL изображения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<Task> tasks = new List<Task>();

            if (!string.IsNullOrWhiteSpace(LeftImageURL) && !IsLeftLoading)
            {
                tasks.Add(StartImageLoadAsync(ImageSide.Left));
            }

            if (!string.IsNullOrWhiteSpace(CenterImageURL) && !IsCenterLoading)
            {
                tasks.Add(StartImageLoadAsync(ImageSide.Center));
            }

            if (!string.IsNullOrWhiteSpace(RightImageURL) && !IsRightLoading)
            {
                tasks.Add(StartImageLoadAsync(ImageSide.Right));
            }

            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
        }

        private void UpdateTotalProgress()
        {
            int activeDownloads = 0;
            int totalProgressSum = 0;

            if (IsLeftLoading)
            {
                activeDownloads++;
                totalProgressSum += LeftProgress;
            }

            if (IsCenterLoading)
            {
                activeDownloads++;
                totalProgressSum += CenterProgress;
            }

            if (IsRightLoading)
            {
                activeDownloads++;
                totalProgressSum += RightProgress;
            }

            if (activeDownloads > 0)
            {
                TotalProgress = (double)totalProgressSum / activeDownloads;
            }
            else
            {
                TotalProgress = 0;
            }
        }
    }
}