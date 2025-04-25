using System;
using System.Data;
using System.Windows;
using MySql.Data.MySqlClient;

namespace aff
{
	public partial class AddEditWindow : Window
	{
		private readonly string _connectionString;
		private readonly string _recordId;

		public AddEditWindow(string connectionString, string recordId)
		{
			InitializeComponent();
			_connectionString = connectionString;
			_recordId = recordId;
			Title = recordId == null ? "Добавить запись" : "Редактировать запись";
			LoadEmployees();

			if (recordId != null)
			{
				LoadRecordData();
			}
			else
			{
				DatePicker.SelectedDate = DateTime.Today;
			}
		}

		private void LoadEmployees()
		{
			try
			{
				using (var conn = new MySqlConnection(_connectionString))
				{
					conn.Open();
					var query = "SELECT EmployeeID as ID, FullName as Name FROM Employees";
					var cmd = new MySqlCommand(query, conn);
					var adapter = new MySqlDataAdapter(cmd);
					var table = new DataTable();
					adapter.Fill(table);
					EmployeeComboBox.ItemsSource = table.DefaultView;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
			}
		}

		private void LoadRecordData()
		{
			try
			{
				using (var conn = new MySqlConnection(_connectionString))
				{
					conn.Open();
					var query = "SELECT r.EmployeeID, r.Date, r.StartTime, r.EndTime " +
								"FROM WorkTimeRecords r " +
								"WHERE r.RecordID = @RecordID";
					var cmd = new MySqlCommand(query, conn);
					cmd.Parameters.AddWithValue("@RecordID", _recordId);

					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							// Устанавливаем выбранного сотрудника
							foreach (DataRowView item in EmployeeComboBox.Items)
							{
								if (item["ID"].ToString() == reader["EmployeeID"].ToString())
								{
									EmployeeComboBox.SelectedItem = item;
									break;
								}
							}

							// Устанавливаем дату
							if (reader["Date"] != DBNull.Value)
							{
								DatePicker.SelectedDate = Convert.ToDateTime(reader["Date"]);
							}

							// Устанавливаем время начала и окончания
							StartTimeTextBox.Text = reader["StartTime"].ToString();
							EndTimeTextBox.Text = reader["EndTime"].ToString();
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки данных записи: {ex.Message}");
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (EmployeeComboBox.SelectedItem == null || DatePicker.SelectedDate == null)
			{
				MessageBox.Show("Выберите сотрудника и дату.");
				return;
			}

			if (!TimeSpan.TryParse(StartTimeTextBox.Text, out var startTime) ||
				!TimeSpan.TryParse(EndTimeTextBox.Text, out var endTime))
			{
				MessageBox.Show("Введите корректное время в формате ЧЧ:MM.");
				return;
			}

			var selectedEmployee = (EmployeeComboBox.SelectedItem as DataRowView).Row;
			var employeeId = selectedEmployee["ID"];
			var date = DatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
			var totalHours = (endTime - startTime).TotalHours;

			try
			{
				using (var conn = new MySqlConnection(_connectionString))
				{
					conn.Open();

					if (_recordId == null)
					{
						// Добавление новой записи
						var query = "INSERT INTO WorkTimeRecords (EmployeeID, Date, StartTime, EndTime, TotalHours) " +
									"VALUES (@EmployeeID, @Date, @StartTime, @EndTime, @TotalHours)";
						var cmd = new MySqlCommand(query, conn);
						cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
						cmd.Parameters.AddWithValue("@Date", date);
						cmd.Parameters.AddWithValue("@StartTime", StartTimeTextBox.Text);
						cmd.Parameters.AddWithValue("@EndTime", EndTimeTextBox.Text);
						cmd.Parameters.AddWithValue("@TotalHours", totalHours);
						cmd.ExecuteNonQuery();
					}
					else
					{
						// Обновление существующей записи
						var query = "UPDATE WorkTimeRecords SET " +
									"EmployeeID = @EmployeeID, " +
									"Date = @Date, " +
									"StartTime = @StartTime, " +
									"EndTime = @EndTime, " +
									"TotalHours = @TotalHours " +
									"WHERE RecordID = @RecordID";
						var cmd = new MySqlCommand(query, conn);
						cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
						cmd.Parameters.AddWithValue("@Date", date);
						cmd.Parameters.AddWithValue("@StartTime", StartTimeTextBox.Text);
						cmd.Parameters.AddWithValue("@EndTime", EndTimeTextBox.Text);
						cmd.Parameters.AddWithValue("@TotalHours", totalHours);
						cmd.Parameters.AddWithValue("@RecordID", _recordId);
						cmd.ExecuteNonQuery();
					}
				}

				DialogResult = true;
				Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка сохранения: {ex.Message}");
			}
		}
	}
}