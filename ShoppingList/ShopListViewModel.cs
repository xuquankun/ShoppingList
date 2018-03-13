using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ShoppingList
{
    class Item
    {
        public string ItemName { get; set; }
        public long id;
        public Item(string name, long iid)
        {
            ItemName = name;
            id = iid;
        }
    }

    class ShopListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string NewProp)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(NewProp));
        }

        private List<Item> _items;
        public List<Item> Items
        {
            get
            { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged("Items");
            }
        }
        private string _itemName;
        public string ItemName
        {
            get
            { return _itemName; }
            set
            {
                _itemName = value;
                OnPropertyChanged("ItemName");
            }
        }
        public Item SelectedItem { get; set; }

        public MyCommand Add { get; set; } = new MyCommand();
        public MyCommand Delete { get; set; } = new MyCommand();

        public ShopListViewModel()
        {
            Add.ExFun = DoAdd;
            Delete.ExFun = DoDelete;

            Items = new List<Item>(SQLRead());
        }

        public List<Item> SQLRead()
        {
            List<Item> a = new List<Item>();
            using (SqlConnection db = new SqlConnection(Properties.Settings.Default.localdb))
            {
                SqlCommand query = new SqlCommand("select item_name, id from Items", db);
                SqlDataReader reader;
                try
                {
                    db.Open();
                    reader = query.ExecuteReader();
                    SqlDataAdapter adapter = new SqlDataAdapter(query);
                    DataTable table = new DataTable();
                    db.Close();
                    adapter.Fill(table);
                    foreach (DataRow row in table.Rows)
                        a.Add(new Item((string)row.ItemArray[0], (long)row.ItemArray[1]));
                }
                catch
                {
                    MessageBox.Show("База данных недоступна");
                }
            }
            return a;
        }

        public void SQLWrite(string request)
        {
            using (SqlConnection db = new SqlConnection(Properties.Settings.Default.localdb))
            {
                SqlCommand query = new SqlCommand(request, db);
                try
                {
                    db.Open();
                    query.ExecuteNonQuery();
                    db.Close();
                }
                catch
                {
                    MessageBox.Show("Не удалось подключиться к базе данных");
                }
            }
        }

        public void DoAdd(object obj)
        {
            if (string.IsNullOrEmpty(ItemName)) return;
            SQLWrite($"insert into Items (item_name) values (\'{ItemName}\')");
            Items = new List<Item>(SQLRead());
            ItemName = string.Empty;
        }

        public void DoDelete(object obj)
        {
            if (SelectedItem == null) return;
            SQLWrite($"delete from Items where id = {SelectedItem.id}");
            Items = new List<Item>(SQLRead());
        }
    }

    public class MyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public Action<object> ExFun { get; set; }
        public bool CanExecute(object parameter) { return true; }
        public void Execute(object parameter)
        {
            ExFun(parameter);
        }
    }
}
