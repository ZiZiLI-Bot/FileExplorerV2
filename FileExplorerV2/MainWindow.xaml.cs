using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;

namespace FileExplorerV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class FileView
    {
        public string STT { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string Date { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        private string filePath;
        private bool isFile = false;
        private string ?currentlySelectedItemName = "";
        private List<string> ListDevices = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            foreach (var drive in DriveInfo.GetDrives())
            {
                ListDevices.Add(drive.Name);
            }
            filePath = ListDevices[0];
            FilePathTextBox.Text = filePath;
            LayoutListDevices.ItemsSource = ListDevices;
            LoadFileAndDirectories();
        }
        public void LoadFileAndDirectories ()
        {
            DirectoryInfo fileList;
            FileAttributes fileAttr;
            OpenDeleteFile.IsEnabled = false;
            ReName.IsEnabled = false;
            try
            {
                if (isFile)
                {
                    string tempFilePath = filePath + "\\" + currentlySelectedItemName;
                    FileInfo fileDetails = new FileInfo(tempFilePath);
                    FileNameLabel.Content = fileDetails.Name;
                    FileTypeLabel.Content = fileDetails.Extension;
                    fileAttr = File.GetAttributes(tempFilePath);
                    Process.Start(new ProcessStartInfo(tempFilePath) { UseShellExecute = true });
                } 
                else
                {
                    fileList = new DirectoryInfo(filePath);
                    FileInfo[] files = fileList.GetFiles(); //Get all file
                    DirectoryInfo[] dirs = fileList.GetDirectories(); // get all forder
                    List<FileView> fileViews = new List<FileView>();
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        fileViews.Add(new FileView()
                        {
                            STT = (i + 1).ToString(),
                            Name = dirs[i].Name,
                            Type = "Folder",
                            Size = "",
                            Date = dirs[i].LastWriteTime.ToString()
                        });
                    }
                    for (int i = 0; i < files.Length; i++)
                    {
                        fileViews.Add(new FileView()
                        {
                            STT = (i + 1).ToString(),
                            Name = files[i].Name,
                            Type = files[i].Extension,
                            Size = files[i].Length.ToString(),
                            Date = files[i].LastWriteTime.ToString()
                        });
                    }
                    ListView.ItemsSource = fileViews;
                }
                }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Không có quyền truy cập!");
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\"));
                FilePathTextBox.Text = filePath;
                LoadBtnAction();
            }
            catch
            {
                MessageBox.Show("Đường dẫn không tồn tại, Vui lòng kiểm tra lại!");
                filePath = filePath.Substring(0, filePath.LastIndexOf("\\"));
                FilePathTextBox.Text = filePath;
                LoadBtnAction();
            } 
        }

        public void goBack()
        {
            try
            {
                string path = FilePathTextBox.Text;
                path = path.Substring(0, path.LastIndexOf("\\"));
                this.isFile = false;
                FilePathTextBox.Text = path;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void LoadBtnAction ()
        {
            filePath = FilePathTextBox.Text;
            LoadFileAndDirectories();
            isFile = false;
        }

        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            LoadBtnAction();
            OpenDeleteFile.IsEnabled = false;
            ReName.IsEnabled = false;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Type? t = ListView?.SelectedItem?.GetType();
            System.Reflection.PropertyInfo[]? props = t?.GetProperties();
            OpenDeleteFile.IsEnabled = true;
            ReName.IsEnabled = true;
            if (props != null)
            {
                currentlySelectedItemName = props[1]?.GetValue(ListView?.SelectedItem).ToString();
                FileAttributes fileAttr = File.GetAttributes(filePath + "\\" + currentlySelectedItemName);
                if ((fileAttr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    isFile = false;
                    FilePathTextBox.Text = filePath + "\\" + currentlySelectedItemName;
                }
                else
                {
                    isFile = true;
                }
            }
        }

        private void ListView_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadBtnAction();
            OpenDeleteFile.IsEnabled = false;
            ReName.IsEnabled = false;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            goBack();
            LoadBtnAction();
        }

        private void LayoutListDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListDevices[LayoutListDevices.SelectedIndex].ToString() != filePath)
            {
                filePath = ListDevices[LayoutListDevices.SelectedIndex].ToString();
                FilePathTextBox.Text = filePath;
                LoadBtnAction();
            }
        }

        private void NewFolderAccset_Click(object sender, RoutedEventArgs e)
        {
            string name = NewFolderNameTextBox.Text;
            if (name != "")
            {
                if (Directory.Exists(filePath + "\\" + name))
                {
                    MessageBox.Show("Thư mục đã tồn tại!");
                }
                else
                {
                    Directory.CreateDirectory(filePath + "\\" + name);
                    NewFolderNameTextBox.Text = "";
                    isFile = false;
                    LoadFileAndDirectories();
                }
            }
            else
            {
                MessageBox.Show("Tên thư mục không được để trống!");
            }
        }

        private void NewFileAccset_Click(object sender, RoutedEventArgs e)
        {
            string name = NewFileNameTextBox.Text;
            if (name != "")
            {
                _ = name.IndexOf(".") == -1 ? name += ".txt" : name;
                if (File.Exists(filePath + "\\" + name))
                {
                    MessageBox.Show("File đã tồn tại!");
                }
                else
                {
                    File.Create(filePath + "\\" + name);
                    NewFileNameTextBox.Text = "";
                    isFile = false;
                    LoadFileAndDirectories();
                }
            }
            else
            {
                MessageBox.Show("Tên tập tin không được để trống!");
            }
        }

        public static void DeleteDirectory(string path) //Goi de quy xoa file
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
          if (!isFile)
            {
                string path = filePath + "\\" + currentlySelectedItemName;
                if (Directory.Exists(path))
                {
                    DeleteDirectory(path);
                    isFile = false;
                    FilePathTextBox.Text = filePath;
                    LoadFileAndDirectories();
                    OpenDeleteFile.IsEnabled = false;
                } 
                else
                {
                    MessageBox.Show("Tập tin không tồn tại");
                }
            }
          else
            {
                string path = filePath + "\\" + currentlySelectedItemName;
                if (File.Exists(path))
                {
                    File.Delete(path);
                    currentlySelectedItemName = "";
                    isFile = false;
                    LoadFileAndDirectories();
                    OpenDeleteFile.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("File không tồn tại!");
                }
            }
        }

        private void ReName_Click(object sender, RoutedEventArgs e)
        {
            if (!isFile)
            {
                ReNameTitle.Text = "Đổi tên thư mục " + currentlySelectedItemName;
            }
            else
            {
                ReNameTitle.Text = "Đổi tên tập tin " + currentlySelectedItemName;
            }
        }

        private void ReNameAccset_Click(object sender, RoutedEventArgs e)
        {
            string name = ReNameTextBox.Text;
            if (!isFile)
            {
                try
                {
                    Directory.Move(filePath + "\\" + currentlySelectedItemName, filePath + "\\" + name);
                }
                catch (IOException)
                {
                    MessageBox.Show("Tên thư mục đã tồn tại!");
                }
                ReNameTextBox.Text = "";
                isFile = false;
                currentlySelectedItemName = "";
                LoadFileAndDirectories();
            }
            else
            {
                _ = name.IndexOf(".") == -1 ? name += currentlySelectedItemName.Substring(currentlySelectedItemName.LastIndexOf(".")) : name;
                try
                {
                    File.Move(filePath + "\\" + currentlySelectedItemName, filePath + "\\" + name);
                }
                catch (IOException)
                {
                    MessageBox.Show("Tên tập tin đã tồn tại!");
                }
                ReNameTextBox.Text = "";
                isFile = false;
                currentlySelectedItemName = "";
                LoadFileAndDirectories();
            }
        }

        private void OpenDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            if (!isFile)
            {
                AccsetDeleteTitle.Text = "Xác nhận xóa thư mục " + currentlySelectedItemName + "?";
            }
            else
            {
                AccsetDeleteTitle.Text = "Xác nhận xóa tập tin " + currentlySelectedItemName + "?";
            }
        }
    }
}
