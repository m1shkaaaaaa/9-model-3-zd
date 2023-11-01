using System.Data.SqlClient;

namespace _6_model_2_zd
{
    internal class Class1
    {
        // Создаем объект SqlConnection для управления соединением с базой данных
        SqlConnection connection = new SqlConnection(@"Data Source = Dmitry; Initial Catalog = TaskDB; Integrated Security = True");

        // Метод для открытия соединения с базой данных
        public void openConnection()
        {
            // Проверяем, закрыто ли соединение
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                // Если соединение закрыто, открываем его
                connection.Open();
            }
        }

        // Метод для закрытия соединения с базой данных
        public void closeConnection()
        {
            // Проверяем, открыто ли соединение
            if (connection.State == System.Data.ConnectionState.Open)
            {
                // Если соединение открыто, закрываем его
                connection.Close();
            }
        }

        // Метод для получения объекта SqlConnection, представляющего соединение
        public SqlConnection getConnection()
        {
            // Возвращаем объект соединения для использования в других частях программы
            return connection;
        }
    }
}
