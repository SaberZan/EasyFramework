using SQLite;
namespace Easy
{
    public class TestSqliteData : SqliteData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}