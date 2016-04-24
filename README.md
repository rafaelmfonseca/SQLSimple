SQLSimple -- Simple CRUD class
<hr>
### Table of Contents
**[Select Query](#select-query)**
**[Insert Query](#insert-query)**
**[Where Conditions](#where-methos)**

### Select Query
Simple example.
```c#
Database db = new Database();
Row[] rows = db.Select("students");
foreach (Row row in rows)
{
    Console.WriteLine("Name = {0}, Age = {1}, City = {2}", row["student_name"], row["student_age"], row["student_city"]);
}
```

### Insert Query
Simple example on a WPF Window.
```c#
public partial class MainWindow : Window
{
    Database db = new Database();

    public MainWindow()
    {
        InitializeComponent();
        db.OnInsert += (s, e) => MessageBox.Show("New row inserted!");
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        db.Insert("students", StudentName.Text, StudentAge.Text, StudentCity.Text);
    }

}
```

### Where Conditions
You have to use LINQ methods to perform where conditions and use another indexer to convert string to int.
```c#
Row[] rows = db.Select("students").Where(r => r["student_age", true] > 17).ToArray();
```
