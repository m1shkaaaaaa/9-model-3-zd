using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using Timer = System.Timers.Timer;

namespace _6_model_2_zd
{
    public partial class Form1 : Form
    {
        SqlConnection connection = new SqlConnection(@"Data Source=Dmitry; Initial Catalog=TaskDB; Integrated Security=True");
        SqlDataAdapter adapter;
        DataTable tasksTable;
        bool isEditing = false; // Флаг для отслеживания режима редактирования
        int editedTaskId = -1; // Идентификатор редактируемой задачи
        private bool notificationSent = false;
        private Timer timer = new Timer();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Инициализируем DataGridView
            tasksTable = new DataTable();
            dataGridView1.DataSource = tasksTable;

            // Загрузим данные из базы данных при запуске приложения
            LoadTasksFromDatabase();
            dateTimePicker1.Format = DateTimePickerFormat.Time;
            dateTimePicker1.ShowUpDown = true; // Показывать только селектор времени, без календаря
            dateTimePicker1.CustomFormat = "HH:mm"; // Формат времени
        }

        private void LoadTasksFromDatabase()
        {
            using (adapter = new SqlDataAdapter("SELECT id AS 'ID', title AS 'Задача', priority AS 'Приоритет', status AS 'Статус', progress AS 'Прогресс', ExecutionTime AS 'Время выполнения' FROM tasks", connection))
            {
                tasksTable.Clear();
                adapter.Fill(tasksTable);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Добавить";
            if (isEditing)
            {
                string taskTitle = textBox1.Text;
                string taskStatus = comboBox1.Text;
                string taskPriority = comboBox2.Text;
                string progressValue = textBox2.Text;

                if (string.IsNullOrWhiteSpace(taskTitle) || string.IsNullOrWhiteSpace(taskStatus) || string.IsNullOrWhiteSpace(taskPriority) || string.IsNullOrWhiteSpace(progressValue))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля: 'Задача', 'Статус', 'Приоритет' и 'Прогресс'.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка корректности ввода значения прогресса
                if (!int.TryParse(progressValue, out int progress) || progress < 0 || progress > 100)
                {
                    MessageBox.Show("Пожалуйста, введите корректное значение для Прогресса (0-100).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (SqlCommand cmd = new SqlCommand("UPDATE tasks SET title = @title, priority = @priority, status = @status, progress = @progress, ExecutionTime = @executionTime WHERE id = @id", connection))
                {
                    cmd.Parameters.AddWithValue("@id", editedTaskId);
                    cmd.Parameters.AddWithValue("@title", taskTitle);
                    cmd.Parameters.AddWithValue("@priority", taskPriority);
                    cmd.Parameters.AddWithValue("@status", taskStatus);
                    cmd.Parameters.AddWithValue("@progress", progress);
                    cmd.Parameters.AddWithValue("@executionTime", dateTimePicker1.Value.TimeOfDay); // Значение времени из DateTimePicker
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }

                isEditing = false;
                editedTaskId = -1;

                LoadTasksFromDatabase(); // Обновить данные в DataGridView после редактирования

                textBox1.Clear();
                comboBox1.SelectedIndex = -1;
                comboBox2.SelectedIndex = -1;
                textBox2.Clear(); // Очистить поле ввода Прогресса
            }
            else
            {
                string taskTitle = textBox1.Text;
                string taskStatus = comboBox1.Text;
                string taskPriority = comboBox2.Text;
                string progressValue = textBox2.Text;

                if (string.IsNullOrWhiteSpace(taskTitle) || string.IsNullOrWhiteSpace(taskStatus) || string.IsNullOrWhiteSpace(taskPriority) || string.IsNullOrWhiteSpace(progressValue))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля: 'Задача', 'Статус', 'Приоритет' и 'Прогресс'.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка корректности ввода значения прогресса
                if (!int.TryParse(progressValue, out int progress) || progress < 0 || progress > 100)
                {
                    MessageBox.Show("Пожалуйста, введите корректное значение для Прогресса (0-100).", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DateTime scheduledTime = dateTimePicker1.Value;

                TimeSpan timeUntilScheduled = scheduledTime - DateTime.Now;

                // Проверка временного интервала перед созданием и запуском таймера и добавлением задачи
                if (timeUntilScheduled.TotalMilliseconds > 0)
                {
                    Timer timer = new Timer();
                    timer.Interval = timeUntilScheduled.TotalMilliseconds;

                    timer.Elapsed += (s, eventArgs) =>
                    {
                        MessageBox.Show("Время выполнения задачи пришло!", "Уведомление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        timer.Stop();
                    };

                    timer.Start();

                    using (SqlCommand cmd = new SqlCommand("INSERT INTO tasks (title, priority, status, progress, ExecutionTime) VALUES (@title, @priority, @status, @progress, @executionTime)", connection))
                    {
                        cmd.Parameters.AddWithValue("@title", taskTitle);
                        cmd.Parameters.AddWithValue("@priority", taskPriority);
                        cmd.Parameters.AddWithValue("@status", taskStatus);
                        cmd.Parameters.AddWithValue("@progress", progress);
                        cmd.Parameters.AddWithValue("@executionTime", dateTimePicker1.Value.TimeOfDay); // Значение времени из DateTimePicker
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }

                    LoadTasksFromDatabase(); // Обновление DataGridView после добавления новой задачи

                    textBox1.Clear();
                    comboBox1.SelectedIndex = -1;
                    comboBox2.SelectedIndex = -1;
                    textBox2.Clear(); // Очистить поле ввода Прогресса
                }
                else
                {
                    MessageBox.Show("Выбранное время уже прошло. Пожалуйста, выберите будущее время.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 1) // Проверка на выбор одной строки для удаления
            {
                int taskId = (int)dataGridView1.SelectedRows[0].Cells["ID"].Value;

                // Удаление задачи из базы данных
                using (SqlCommand cmd = new SqlCommand("DELETE FROM tasks WHERE id = @id", connection))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }

                // Обновление DataGridView
                LoadTasksFromDatabase();
            }
            else if (dataGridView1.SelectedRows.Count > 1) // Если выбрано более одной строки
            {
                MessageBox.Show("Нельзя выбрать несколько элементов для удаления. Пожалуйста, выберите только одну строку.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            // Предупреждение перед удалением всех данных
            DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить все задачи?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Удаление всех задач из базы данных
                using (SqlCommand cmd = new SqlCommand("DELETE FROM tasks", connection))
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }

                // Обновление DataGridView после удаления
                LoadTasksFromDatabase();
            }
        }
        
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                isEditing = true; // Включить режим редактирования
                editedTaskId = (int)dataGridView1.Rows[e.RowIndex].Cells["ID"].Value;
                textBox1.Text = dataGridView1.Rows[e.RowIndex].Cells["Задача"].Value.ToString();
                comboBox1.Text = dataGridView1.Rows[e.RowIndex].Cells["Статус"].Value.ToString();
                comboBox2.Text = dataGridView1.Rows[e.RowIndex].Cells["Приоритет"].Value.ToString(); // Загрузка приоритета обратно в comboBox2

                // Изменяем текст на кнопке
                button1.Text = "Отредактировать";
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}