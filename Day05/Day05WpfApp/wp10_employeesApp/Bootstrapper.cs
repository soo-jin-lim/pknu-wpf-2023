using Caliburn.Micro;
using System.Windows;
//using wp09_caliburnApp.ViewModels;
using wp10_employeesApp.ViewModels;
namespace wp10_employeesApp
{
    //Caliburn으로 MVVM시작할 때 주요설정 실행
    public class Bootstrapper: BootstrapperBase
    {
         public Bootstrapper()
        { 
            Initialize(); // Caliburn MVVM 초기화
        }
        protected async override void OnStartup(object sender, StartupEventArgs e)
        {
            // base.OnStartup(sender, e); 부모클래스 실행은 주석
            await DisplayRootViewForAsync<MainViewModel>();
        }
    }
}
