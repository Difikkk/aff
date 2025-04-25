using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace aff
{
	public partial class MainWindow : Window
	{
		private string connectionString = "server=localhost;user=root;password=root;database=WorkTimeDB1";

		public MainWindow()
		{
			InitializeComponent();
			LoadData();
		}

		private void LoadData()
		{
			try
			{
				using (var conn = new MySqlConnection(connectionString))
				{
					conn.Open();
					var query = "SELECT r.RecordID, e.FullName, r.Date, r.StartTime, r.EndTime, r.TotalHours " +
								"FROM WorkTimeRecords r " +
								"JOIN Employees e ON r.EmployeeID = e.EmployeeID";
					var adapter = new MySqlDataAdapter(query, conn);
					var table = new DataTable();
					adapter.Fill(table);
					WorkTimeDataGrid.ItemsSource = table.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
			}
		}

		private void AddRecord_Click(object sender, RoutedEventArgs e)
		{
			var addWindow = new AddEditWindow(connectionString, null);
			if (addWindow.ShowDialog() == true)
			{
				LoadData();
			}
		}

		private void EditRecord_Click(object sender, RoutedEventArgs e)
		{
			if (WorkTimeDataGrid.SelectedItem == null)
			{
				MessageBox.Show("Выберите запись для редактирования.");
				return;
			}

			var selectedRow = (WorkTimeDataGrid.SelectedItem as DataRowView).Row;
			var recordId = selectedRow["RecordID"].ToString();
			var editWindow = new AddEditWindow(connectionString, recordId);
			if (editWindow.ShowDialog() == true)
			{
				LoadData();
			}
		}

		private void DeleteRecord_Click(object sender, RoutedEventArgs e)
		{
			if (WorkTimeDataGrid.SelectedItem == null)
			{
				MessageBox.Show("Выберите запись для удаления.");
				return;
			}

			var selectedRow = (WorkTimeDataGrid.SelectedItem as DataRowView).Row;
			var recordId = selectedRow["RecordID"].ToString();

			if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение удаления",
				MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				try
				{
					using (var conn = new MySqlConnection(connectionString))
					{
						conn.Open();
						var query = "DELETE FROM WorkTimeRecords WHERE RecordID = @RecordID";
						var cmd = new MySqlCommand(query, conn);
						cmd.Parameters.AddWithValue("@RecordID", recordId);
						cmd.ExecuteNonQuery();
					}

					LoadData();
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Ошибка удаления записи: {ex.Message}");
				}
			}
		}

		private void ViewReport_Click(object sender, RoutedEventArgs e)
		{
			var reportWindow = new ReportWindow(connectionString);
			reportWindow.ShowDialog();
		}
	}
}