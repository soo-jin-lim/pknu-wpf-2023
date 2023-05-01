using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using wp11_Movie.Logics;
using wp11_movieFinder.Models;

namespace wp11_Movie
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private async void BtnNaverMovie_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await Commons.showMessageAsync("네이버 영화", "네이버영화 사이트로 갑니다.");
        }

        private async void BtnSearchMovie_Click(object sender, RoutedEventArgs e)
        {
           if(string.IsNullOrEmpty(TxtMovieName.Text))
            {
                await Commons.showMessageAsync("검색", "검색할 영화명을 입력하세요");
                return;
            }
           //if (TxtMovieName.Text.Length <= 2)
            //{
              //  await Commons.showMessageAsync("검색", "검색어를 2자 이상 입력하세요.");
                //return;
            //}
           try
            {
                SearchMovie(TxtMovieName.Text);
            }
            catch(Exception ex)
            {
                await Commons.showMessageAsync("오류", $"오류 발생 : {ex.Message}");
            }
        }


        

        // 텍스트 박스에서 키를 입력할 때 엔테를 누르면 검색시작
        private void TxtMovieName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                BtnSearchMovie_Click(sender, e);
              
            }

        }
        private async void SearchMovie(string movieName)
        {
            string tmdb_apiKey= "9725ec64fef5454261825594403926fd";
            string encoding_MovieName = HttpUtility.UrlEncode(movieName, Encoding.UTF8);
            string openApiUri = $@"https://api.themoviedb.org/3/search/movie?api_key={tmdb_apiKey}" +
                                 $"&language=ko-KR&page=1&include_adult=false&query={encoding_MovieName}";
            string result = string.Empty;

            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            //TMDB 요청
            try
            {
                req = WebRequest.Create(openApiUri);
                res = req.GetResponse();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd(); // 

                Debug.WriteLine(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                res.Close();
            }

            var jsonResult = JObject.Parse(result); //string -> jason

            var total = Convert.ToInt32(jsonResult["total_results"]);
            //await Commons.showMessageAsync("검색결과", total.ToString());
            var items = jsonResult["results"];

            var json_array = items as JArray;

            var movieItems = new List<MovieItem>(); // json에서 넘어온 배열을 담을 장소
            foreach (var val in json_array)
            {
                var MovieItem = new MovieItem()
                {
                    Id = Convert.ToInt32(val["id"]),
                    Title = Convert.ToString(val["title"]),
                    Original_Title = Convert.ToString(val["original_title"]),
                    Release_Date = Convert.ToString(val["release_date"]),
                    Vote_Average = Convert.ToDouble(val["vote_average"])
                };
                movieItems.Add(MovieItem);
            }

            this.DataContext = movieItems;

        }
    }
}
