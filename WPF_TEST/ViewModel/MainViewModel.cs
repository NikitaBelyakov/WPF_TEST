/*
    Добавлена автоматизация создания экземпляров для N изображений
    Свойства изображений вынесены в отдельный ImageStates.cs
    Создан User-компонент, в который вынесен шаблон загрузчика изображений
    В MainWindow автоматически создается заданное кол-во загрузчиков
    
    Если необходимо, то можно добавить пользовательский ввод кол-ва изображений и обновлять набор загрузчиков из интерфейса
    Также, добавил анимацию пульсации на поле ввода ссылки (если оно пустое) для визуального выделения 
*/

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
using System.Timers;
using System.Net.Http;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Threading;
using System.ComponentModel;

namespace WPF_TEST.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<ImageStates> ImageStates { get; }

        private double _totalProgress;
        public double TotalProgress
        {
            get => _totalProgress;
            set => SetProperty(ref _totalProgress, value);
        }

        private double _imageCount = 3;
        public double ImageCount
        {
            get => _imageCount;
            set => SetProperty(ref _imageCount, value);
        }

        public RelayCommand<ImageStates> LoadImageCommand { get; }
        public IRelayCommand<ImageStates> StopImageCommand { get; }
        public IAsyncRelayCommand LoadAllCommand { get; }

        public MainViewModel()
        {
            ImageStates = new ObservableCollection<ImageStates>();

            for (int i = 0; i < ImageCount; i++)
            {
                ImageStates.Add(new ImageStates());
            }


            LoadImageCommand = new RelayCommand<ImageStates>(async (item) =>
            {
                await StartImageLoadAsync(item);
            }, (item) => true);

            StopImageCommand = new RelayCommand<ImageStates>(StopImageLoad, (item) => true);

            LoadAllCommand = new AsyncRelayCommand(LoadAllImagesAsync);
        }

        private async Task StartImageLoadAsync(ImageStates? item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ImageURL))
            {
                MessageBox.Show("Введите URL изображения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DownloadedImages");
            if (!Directory.Exists(imagesFolder)) Directory.CreateDirectory(imagesFolder);

            string fileName = $"{Guid.NewGuid():N}.jpg";
            string filePath = Path.Combine(imagesFolder, fileName);

            item.CancellationTokenSource?.Cancel();
            item.CancellationTokenSource?.Dispose();
            item.CancellationTokenSource = new CancellationTokenSource();

            item.IsLoading = true;
            item.Progress = 0;
            item.IsLoaded = false;

            try
            {
                var bitmapImage = await DownloadImageAsync(item.ImageURL, filePath, item.CancellationTokenSource.Token, item);

                if (!item.CancellationTokenSource.IsCancellationRequested && bitmapImage != null)
                {
                    item.ImageSource = bitmapImage;
                    item.Progress = 100;
                    item.IsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                item.IsLoading = false;
                UpdateTotalProgress();
            }
        }

        private async Task<BitmapImage?> DownloadImageAsync(string url, string filePath, CancellationToken token, ImageStates item)
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1L;
            long receivedBytes = 0;

            using (var stream = await response.Content.ReadAsStreamAsync(token))
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                byte[] buffer = new byte[8192];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                    receivedBytes += bytesRead;

                    if (totalBytes > 0)
                    {
                        item.Progress = (int)((double)receivedBytes / totalBytes * 100);
                        UpdateTotalProgress();
                    }
                }
            }

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.UriSource = new Uri(filePath);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        private void StopImageLoad(ImageStates? item)
        {
            if (item == null) return;
            item.CancellationTokenSource?.Cancel();
            item.IsLoading = false;
            item.Progress = 0;
            UpdateTotalProgress();
        }

        private async Task LoadAllImagesAsync()
        {
            if (!ImageStates.Any(item => !string.IsNullOrWhiteSpace(item.ImageURL)))
            {
                MessageBox.Show("Введите URL изображения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tasks = ImageStates
                .Where(item => !string.IsNullOrWhiteSpace(item.ImageURL) && !item.IsLoading)
                .Select(item => StartImageLoadAsync(item));

            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }
            else
            {
                MessageBox.Show("Введите хотя бы один URL изображения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateTotalProgress()
        {
            var activeItems = ImageStates.Where(x => x.IsLoading).ToList();
            TotalProgress = activeItems.Any() ? activeItems.Average(x => x.Progress) : 0;
        }
    }
}