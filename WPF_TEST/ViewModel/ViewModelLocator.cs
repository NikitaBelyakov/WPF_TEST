using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_TEST.ViewModel;

namespace WPF_TEST.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<MainViewModel>();

            Ioc.Default.ConfigureServices(serviceCollection.BuildServiceProvider());
        }

        public MainViewModel Main
        {
            get
            {
                return Ioc.Default.GetService<MainViewModel>();
            }
        }

        public static void Cleanup()
        {

        }
    }
}
