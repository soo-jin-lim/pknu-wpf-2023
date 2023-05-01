using Api.Logics;
using Api.Models;
using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;


namespace Api
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

        

        // 농림축산검역본부_동물등록 현황 openAPI 조회
        private async void BtnReqRealtime_Click(object sender, RoutedEventArgs e)
        {
            string openApiUri = "http://211.237.50.150:7080/openapi/sample/json/Grid_20210806000000000612_1/1/5";
            string result = string.Empty;
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;
            try
            {
                // WebRequest, WebResponse 객체
                req = WebRequest.Create(openApiUri);
                res = await req.GetResponseAsync();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"OpenAPI 조회오류 {ex.Message}");
            }

            var jsonResult = JObject.Parse(result);
            //var status = Convert.ToInt32(jsonResult["status"]);

            try
            {
                var data = jsonResult["Grid_20210806000000000612_1"]["row"];
                var json_array = data as JArray;

                var animal = new List<Info>();
                foreach (var sensor in json_array)
                {
                    animal.Add(new Info
                    {
                        ROW_NUM = Convert.ToInt32(sensor["ROW_NUM"]), // openAPI
                        CTPV = Convert.ToString(sensor["CTPV"]),
                        SGG = Convert.ToString(sensor["SGG"]),
                        BRDT = Convert.ToString(sensor["BRDT"]),
                        RFID_SE = Convert.ToString(sensor["RFID_SE"]),
                        LVSTCK_KND = Convert.ToString(sensor["LVSTCK_KND"]),
                        SPCS = Convert.ToString(sensor["SPCS"]),
                        CNT = Convert.ToInt32(sensor["CNT"]),
                    });                    
                }
                this.DataContext = animal;
                StsResult.Content = $"OpenAPI {animal.Count}건 조회완료";

            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"JSON 처리오류 {ex.Message}");
            }
        }

        // 검색한 결과 DB(MySQL)에 저장
        private async void BtnSaveData_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.Items.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "조회하고 저장하세요.");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();
                    var query = @"INSERT INTO animal
                                        (Row_Num,
                                         CTPV,
                                         SGG,
                                         BRDT,
                                         RFID_SE,
                                         LVSTCK_KND,
                                         SPCS,
                                         CNT)
                                        VALUES
                                        (@Row_Num,
                                         @CTPV,
                                         @SGG,
                                         @BRDT,
                                         @RFID_SE,
                                         @LVSTCK_KND,
                                         @SPCS,
                                         @CNT)";

                    var insRes = 0;
                    foreach (var temp in GrdResult.Items)
                    {
                        if (temp is Info)
                        {
                            var item = temp as Info;

                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@ROW_NUM", item.ROW_NUM);
                            cmd.Parameters.AddWithValue("@CTPV", item.CTPV);
                            cmd.Parameters.AddWithValue("@SGG", item.SGG);
                            cmd.Parameters.AddWithValue("@BRDT", item.BRDT);
                            cmd.Parameters.AddWithValue("@RFID_SE", item.RFID_SE);
                            cmd.Parameters.AddWithValue("@LVSTCK_KND", item.LVSTCK_KND);
                            cmd.Parameters.AddWithValue("@SPCS", item.SPCS);
                            cmd.Parameters.AddWithValue("@CNT", item.CNT);

                            insRes += cmd.ExecuteNonQuery();
                        }
                    }

                    await Commons.ShowMessageAsync("저장", "DB저장 성공!!");
                    StsResult.Content = $"DB저장 {insRes}건 성공";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류! {ex.Message}");
            }

        }

        private void GrdResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void CboReqDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CboReqDate.SelectedValue != null)
            {
                //MessageBox.Show(CboReqDate.SelectedValue.ToString());
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    conn.Open();
                    var query = @"SELECT Row_Num,
                                         CTPV,
                                         SGG,
                                         BRDT,
                                         RFID_SE,
                                         LVSTCK_KND,
                                         SPCS,
                                         CNT
                                        FROM animal
                                        WHERE SPCS = @SPCS";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SPCS", CboReqDate.SelectedValue.ToString());
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, "animal");
                    List<Info> animal = new List<Info>();
                    foreach (DataRow row in ds.Tables["animal"].Rows)
                    {
                        animal.Add(new Info
                        {
                            ROW_NUM = Convert.ToInt32(row["ROW_NUM"]), // openAPI
                            CTPV = Convert.ToString(row["CTPV"]),
                            SGG = Convert.ToString(row["SGG"]),
                            BRDT = Convert.ToString(row["BRDT"]),
                            RFID_SE = Convert.ToString(row["RFID_SE"]),
                            LVSTCK_KND = Convert.ToString(row["LVSTCK_KND"]),
                            SPCS = Convert.ToString(row["SPCS"]),
                            CNT = Convert.ToInt32(row["CNT"]),
                        });
                    }

                    this.DataContext = animal;
                    StsResult.Content = $"DB {animal.Count}건 조회완료";
                }
            }
            else
            {
                this.DataContext = null;
                StsResult.Content = $"DB 조회클리어";
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    conn.Open();
                    var query = @"SELECT SPCS
                                FROM animal
                               GROUP BY 1
                               ORDER BY 1 ";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    List<string> saveDateList = new List<string>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        saveDateList.Add(Convert.ToString(row["SPCS"]));
                    }
                    CboReqDate.ItemsSource = saveDateList;
                }
            }
    }
}