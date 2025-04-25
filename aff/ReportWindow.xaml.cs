using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace aff
{
	public partial class ReportWindow : Window
	{
		private readonly string _connectionString;

		public ReportWindow(string connectionString)
		{
			InitializeComponent();
			_connectionString = connectionString;
			StartDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
			EndDatePicker.SelectedDate = DateTime.Today;
		}

		private void GenerateReport_Click(object sender, RoutedEventArgs e)
		{
			if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
			{
				MessageBox.Show("Выберите период для отчета.");
				return;
			}

			try
			{
				using (var conn = new MySqlConnection(_connectionString))
				{
					conn.Open();
					var query = @"SELECT e.FullName, 
                                SUM(r.TotalHours) as TotalHours,
                                COUNT(r.RecordID) as WorkDays
                                FROM WorkTimeRecords r
                                JOIN Employees e ON r.EmployeeID = e.EmployeeID
                                WHERE r.Date BETWEEN @StartDate AND @EndDate
                                GROUP BY e.FullName
                                ORDER BY e.FullName";

					var cmd = new MySqlCommand(query, conn);
					cmd.Parameters.AddWithValue("@StartDate", StartDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
					cmd.Parameters.AddWithValue("@EndDate", EndDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));

					var adapter = new MySqlDataAdapter(cmd);
					var table = new DataTable();
					adapter.Fill(table);
					ReportDataGrid.ItemsSource = table.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка формирования отчета: {ex.Message}");
			}
		}
	}
}