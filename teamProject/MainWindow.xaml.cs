using Microsoft.VisualBasic;
using Microsoft.Win32;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using teamProject.Models;

namespace teamProject
{
    public partial class MainWindow : Window
    {
        private XamlModel model;
        private string openedDirectory;
        private string _soursDirectory;
        private bool isMove = false;
        private List<CancelTaskToken> tokens;
        private UpdateContext context;
        private string fName = Directory.GetCurrentDirectory() + @"\save.dat";

        public MainWindow()
        {
            InitializeComponent();
            model = new XamlModel();
            this.DataContext = model;
            openedDirectory = null!;
            _soursDirectory = null!;
            tokens = new List<CancelTaskToken>();

            context = new UpdateContext(model, ItemsListBox, FolderListBox, tokens, BackBtn, NextBtn);

            GetFileData();
        }

        private void SaveFileData(string currentDirectory)
        {
            using (FileStream fs = new FileStream(fName, FileMode.Append, FileAccess.Write)) {
                byte[] array = Encoding.UTF8.GetBytes(currentDirectory + Environment.NewLine);
                fs.Write(array, 0, array.Length);
            }
        }

        private void GetFileData()
        {
            using (StreamReader fs = new StreamReader(new FileStream(fName, FileMode.OpenOrCreate, FileAccess.Read)))
            {
                HashSet<string> fileData = fs.ReadToEnd().Split('\n').ToHashSet();
                fileData.Remove("");

                if (fileData.Count == 0)
                {
                    GetDefaultPath();
                    ItemsUpdater.Update(context);
                }
                else
                {
                    List<string> list = fileData.ToList();
                    model.Path = "Історія відкритих папок";
                    List<DItem> items = new List<DItem>();

                    while (list.Count > 10)
                    {
                        list.RemoveAt(0);
                    }

                    foreach (var item in list)
                    {
                        DDirectory dItem = new DDirectory(System.IO.Path.GetFileName(item), item.Trim(), "Assets/folder.png");
                        items.Add(dItem);
                    }
                    ItemsListBox.ItemsSource = items;
                    TotalSizeUpdater.Update(context);
                    model.UpdateCount(ItemsListBox.Items.Count);
                }
            }
        }

        private void GetDefaultPath()
        {
            model.Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openedDirectory = model.Path;

        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Пошук")
            {
                SearchTextBox.Text = "";
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Пошук";
            }
        }

        private void ItemGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                DItem dItem = (DItem)ItemsListBox.SelectedItem;

                if (dItem is DDrive)
                {
                    OpenDrive(dItem);
                }
                else if (dItem is DDirectory)
                {
                    OpenDirectory(dItem);
                }
                else if (dItem is DFile)
                {
                    OpenFile(dItem);
                }
            }
        }

        private void OpenDrive(DItem dItem)
        {
            model.PushBackPath(model.Path);
            model.Path = ((DDrive)dItem).Path;
            Directory.SetCurrentDirectory(model.Path);
            openedDirectory = Directory.GetCurrentDirectory();
            model.forwardPathHistory.Clear();

            ItemsUpdater.Update(context);
        }

        private void OpenDirectory(DItem dItem)
        {
            try
            {
                model.PushBackPath(model.Path);
                model.Path = dItem.Path;
                Directory.SetCurrentDirectory(model.Path);
                openedDirectory = Directory.GetCurrentDirectory();
                model.forwardPathHistory.Clear();

                ItemsUpdater.Update(context);
                SaveFileData(openedDirectory);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Ви не маєте доступу до цієї папки.", "Неавторизований доступ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show($"Не можливо знайти шлях до папки.", "Невірне розташування", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непередбачена помилка: {ex.Message}", "Помилка шляху папки");
            }
        }

        private void OpenFile(DItem dItem)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.UseShellExecute = true;
            startInfo.FileName = dItem.Path;
            process.StartInfo = startInfo;

            process.Start();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo parentDir = Directory.GetParent(model.Path)!;

            if (parentDir != null)
            {
                model.PushForwardPath(model.Path);
                model.Path = parentDir.FullName;
                Directory.SetCurrentDirectory(model.Path);
                openedDirectory = Directory.GetCurrentDirectory();

                ItemsUpdater.Update(context);
            }
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (model.CanGoForward())
            {
                string nextPath = model.PopForwardPath();

                if (Directory.Exists(nextPath))
                {
                    model.Path = nextPath;
                    Directory.SetCurrentDirectory(model.Path);
                    openedDirectory = Directory.GetCurrentDirectory();

                    ItemsUpdater.Update(context);
                }
                else
                {
                    model.RemoveForwardPath(nextPath);
                    NextBtn.IsEnabled = false;

                    MessageBox.Show("Цей шлях більше не існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void CreateFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*|Folder (*)|*";
            saveFileDialog.FilterIndex = 3;

            string currentDirectory = openedDirectory;
            saveFileDialog.InitialDirectory = currentDirectory;

            bool? res = saveFileDialog.ShowDialog();

            if (res == true)
            {
                try
                {
                    string uniqueFileName = GetUniqueFileName(saveFileDialog.FileName);

                    if (saveFileDialog.FilterIndex != 3)
                    {
                        using (File.Create(uniqueFileName)) { }
                    }
                    else
                    {
                        Directory.CreateDirectory(uniqueFileName);
                    }

                    ItemsUpdater.Update(context);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка створення файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedFiles = ItemsListBox.SelectedItems;
                var fileListForClipboard = new StringCollection();
                string filePath = "";

                foreach (DItem selectedFile in selectedFiles)
                {
                    filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), selectedFile.Name);
                    fileListForClipboard.Add(filePath);
                }

                _soursDirectory = filePath;
                Clipboard.SetFileDropList(fileListForClipboard);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка копіювання файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            pasteItem.IsEnabled = true;
        }

        private async void Paste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filesToPaste = Clipboard.GetFileDropList();
                string[] fileArray = new string[filesToPaste.Count];
                filesToPaste.CopyTo(fileArray, 0);

                foreach (var fileToPaste in fileArray)
                {

                    if (PathTextBox.Text.Contains(System.IO.Path.GetFileName(fileToPaste)))
                    {
                        MessageBox.Show("Не можливо вставити папку саму в себе", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);

                        continue;
                    }

                    if (File.Exists(fileToPaste))
                    {
                        await PasteFileAsync(fileToPaste);
                    }
                    else if (Directory.Exists(fileToPaste))
                    {
                        await PasteDirectoryAsync(fileToPaste);
                    }
                }

                ItemsUpdater.Update(context);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка вставки файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Task PasteFileAsync(string filePath)
        {
            return Task.Run(async () =>
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                var destinationPath = System.IO.Path.Combine(openedDirectory, fileName);
                var uniqueFileName = GetUniqueFileName(fileName);

                File.Copy(filePath, uniqueFileName);

                if (isMove)
                {
                    await DeleteFileAsync(filePath);

                    isMove = false;
                }
            });
        }

        private Task PasteDirectoryAsync(string dirPath)
        {
            return Task.Run(async () =>
            {
                var sourceDirectoryName = new DirectoryInfo(dirPath).Name;
                var destinationPath = System.IO.Path.Combine(openedDirectory, sourceDirectoryName);
                var uniqueDirectoryName = GetUniqueFileName(sourceDirectoryName);

                if (openedDirectory == _soursDirectory)
                {
                    MessageBox.Show("Не можливо вставити папку саму в себе", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Directory.CreateDirectory(uniqueDirectoryName);

                    await CopyDirectoryAsync(dirPath, destinationPath);

                    if (isMove)
                    {
                        await DeleteDirectoryAsync(dirPath);

                        isMove = false;
                    }
                }
            });
        }

        private string GetUniqueFileName(string fileName)
        {
            string directoryPath = Directory.GetCurrentDirectory();
            string extension = System.IO.Path.GetExtension(fileName);
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
            string newName = fileName;

            if (File.Exists(System.IO.Path.Combine(directoryPath, newName)))
            {
                int count = 2;

                while (File.Exists(System.IO.Path.Combine(directoryPath, newName)))
                {
                    newName = $"{fileNameWithoutExtension}({count}){extension}";
                    count++;
                }
            }

            return newName;
        }

        private Task CopyDirectoryAsync(string sourceDirectoryName, string destinationDirectoryName)
        {
            return Task.Run(async () =>
            {
                var directory = new DirectoryInfo(sourceDirectoryName);
                var dirs = directory.GetDirectories();
                var files = directory.GetFiles();

                Directory.CreateDirectory(destinationDirectoryName);

                foreach (var file in files)
                {
                    var tempPath = System.IO.Path.Combine(destinationDirectoryName, file.Name);
                    file.CopyTo(tempPath, false);
                }

                foreach (var subDir in dirs)
                {
                    var tempPath = System.IO.Path.Combine(destinationDirectoryName, subDir.Name);

                    await CopyDirectoryAsync(subDir.FullName, tempPath);
                }
            });
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = new List<DItem>(ItemsListBox.SelectedItems.Cast<DItem>());

            try
            {
                TryDeleteItems(selectedItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TryDeleteItems(List<DItem> Items)
        {
            foreach (DItem selectedFile in Items)
            {
                var itemPath = System.IO.Path.Combine(openedDirectory, selectedFile.Name);

                if (File.Exists(itemPath))
                {
                    await DeleteFileAsync(itemPath);
                }
                else if (Directory.Exists(itemPath))
                {
                    await DeleteDirectoryAsync(itemPath);
                    DeleteDirIfEmpty(itemPath);
                }

                ItemsUpdater.Update(context);
            }
        }

        private Task DeleteFileAsync(string filePath)
        {
            return Task.Run(() =>
            {
                FileInfo fi = new FileInfo(filePath);

                fi.IsReadOnly = false;
                fi.Delete();
            });
        }

        private Task DeleteDirectoryAsync(string curDirPath)
        {
            return Task.Run(async () =>
            {
                DeleteDirectoryFiles(curDirPath);

                string[] directories = Directory.GetDirectories(curDirPath);

                foreach (string dirPath in directories)
                {
                    await DeleteDirectoryAsync(dirPath);
                }

                DeleteDirIfEmpty(curDirPath);
            });
        }

        private void DeleteDirectoryFiles(string dirPath)
        {
            string[] files = Directory.GetFiles(dirPath);

            foreach (string filePath in files)
            {
                FileInfo fi = new FileInfo(filePath);

                fi.IsReadOnly = false;
                fi.Delete();
            }
        }

        private void DeleteDirIfEmpty(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                string[] dirFiles = Directory.GetFiles(dirPath);
                string[] dirDirectories = Directory.GetDirectories(dirPath);

                if (dirFiles.Length == 0 && dirDirectories.Length == 0)
                {
                    DirectoryInfo di = new DirectoryInfo(dirPath);
                    di.Delete();
                }
            }
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ItemsListBox.SelectedItem is DFile selectedFile)
                {
                    var oldFilePath = System.IO.Path.Combine(openedDirectory, selectedFile.Name);
                    var newFileName = Interaction.InputBox("Введіть нову назву файлу:", "Перейменувати файл", selectedFile.Name);

                    if (!string.IsNullOrWhiteSpace(newFileName))
                    {
                        var newFilePath = System.IO.Path.Combine(openedDirectory, newFileName);

                        File.Move(oldFilePath, newFilePath);
                        ItemsUpdater.Update(context);
                    }
                }
                else if (ItemsListBox.SelectedItem is DDirectory selectedDirectory)
                {
                    var oldDirectoryPath = System.IO.Path.Combine(openedDirectory, selectedDirectory.Name);
                    var newDirectoryName = Interaction.InputBox("Введіть нову назву папки:", "Перейменувати папку", selectedDirectory.Name);

                    if (!string.IsNullOrWhiteSpace(newDirectoryName))
                    {
                        var newDirectoryPath = System.IO.Path.Combine(openedDirectory, newDirectoryName);

                        if (!Directory.Exists(newDirectoryPath))
                        {
                            Directory.Move(oldDirectoryPath, newDirectoryPath);
                            ItemsUpdater.Update(context);
                        }
                        else
                        {
                            MessageBox.Show("Папка з такою назвою вже існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Виберіть файл або папку для перейменування.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка перейменування: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Mov_Click(object sender, RoutedEventArgs e)
        {
            isMove = true;

            Copy_Click(sender, e);
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Directory.SetCurrentDirectory(model.Path);
            openedDirectory = Directory.GetCurrentDirectory();
            pasteItem.IsEnabled = false;

            ItemsUpdater.Update(context);
        }

        private void UpDate_btn(object sender, RoutedEventArgs e)
        {
            pasteItem.IsEnabled = false;

            ItemsUpdater.Update(context);
        }

        private void Home_btn(object sender, RoutedEventArgs e)
        {
            model.Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Directory.SetCurrentDirectory(model.Path);
            openedDirectory = Directory.GetCurrentDirectory();

            ItemsUpdater.Update(context);
        }

        private void MyFolder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DDirectory selectedDir = (DDirectory)FolderListBox.SelectedItem;

            if (e.ClickCount >= 2 && model.Path != selectedDir.Path)
            {
                if (selectedDir.Path == "<=Discs=>")
                {
                    DrivesUpdater.Update(context);
                }
                else
                {
                    model.Path = selectedDir.Path;
                    Directory.SetCurrentDirectory(selectedDir.Path);
                    openedDirectory = Directory.GetCurrentDirectory();
                    ItemsUpdater.Update(context);
                }
            }
        }

        private void PathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    model.Path = PathTextBox.Text;
                    Directory.SetCurrentDirectory(model.Path);

                    ItemsUpdater.Update(context);
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show("Вказано невірний шлях.", "Помилка шляху", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Непердбачена помилка шляху: {ex.Message}", "Помилка шляху", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (CancelTaskToken token in tokens)
            {
                token.Cancel();
            }

            tokens.Clear();
        }

        private void FolderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            string phrase = SearchTextBox.Text.Trim();

            if (phrase != "Пошук")
            {
                model.ClearItems();

                if (string.IsNullOrEmpty(phrase))
                {
                    ItemsUpdater.Update(context);
                }
                else
                {
                    SearchDirectories(model.Path, phrase);
                }
            }
        }

        private async void SearchDirectories(string rootPath, string phrase)
        {
            List<string> foundItems = new List<string>();

            await Task.Run(() =>
            {
                try
                {
                    foundItems.AddRange(Directory
                        .EnumerateFileSystemEntries(rootPath, "*.*")
                        .Where(path => System.IO.Path.GetFileName(path)
                        .Contains(phrase, StringComparison.OrdinalIgnoreCase)));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Неможливо доступитися до {rootPath}: {ex.Message}");
                }
            });

            await ItemsByTypeUpdater.Update(foundItems.ToArray(), context);

        }

        private void Sort_btn(object sender, RoutedEventArgs e)
        {
            if (SortButton.ContextMenu != null)
            {
                SortButton.ContextMenu.PlacementTarget = SortButton;
                SortButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                SortButton.ContextMenu.IsOpen = true;
            }
        }

        private void SortFromA(object sender, RoutedEventArgs e)
        {
            model.ClearItems();
            ItemSorter.SortFromAtoZ(context);
        }

        private void SortFromZ(object sender, RoutedEventArgs e)
        {
            model.ClearItems();
            ItemSorter.SortFromZtoA(context);
        }

        private void SortByDate(object sender, RoutedEventArgs e)
        {
            model.ClearItems();
            ItemSorter.SortDate(context);
        }

        private void SortBySize(object sender, RoutedEventArgs e)
        {
            model.ClearItems();
            ItemSorter.SortSize(context);
        }

        private void SortDateRev(object sender, RoutedEventArgs e)
        {
            model.ClearItems();
            ItemSorter.SortDateDesc(context);
        }
        private void SortSizeRev(object sender, RoutedEventArgs e)
        {
            model.ClearItems();
            ItemSorter.SortSizeDesc(context);
        }

        private void OpenWithNotepad_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            string fname = ((DItem)ItemsListBox.SelectedItem).Path;

            ProcessStartInfo startInfo = new ProcessStartInfo{
                UseShellExecute = true,
                FileName = "Notepad.exe",
                Arguments = $"\"{fname}\""
            };

            process.StartInfo = startInfo;
            process.Start();
        }

        private void OpenWithPaint_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            string fname = ((DItem)ItemsListBox.SelectedItem).Path;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "mspaint.exe",
                Arguments = $"\"{fname}\""
            };

            process.StartInfo = startInfo;
            process.Start();
        }
        
        private void OpenWithEdge_Click(object sender, RoutedEventArgs e)
        {
            Process process = new Process();
            string fname = ((DItem)ItemsListBox.SelectedItem).Path;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = "msedge.exe",
                Arguments = $"\"{fname}\""
            };

            process.StartInfo = startInfo;
            process.Start();
        }

        private void OpenOtherProgram_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true) {
                string fname = ((DItem)ItemsListBox.SelectedItem).Path;
                Process.Start(openFileDialog.FileName, $"\"{fname}\"");
            }
        }

        private void CreateArchive_Click(object sender, RoutedEventArgs e)
        {
            List<DItem> selectedItems = new List<DItem>(ItemsListBox.SelectedItems.Cast<DItem>());
            string fname2 = ((DItem)ItemsListBox.SelectedItem).Path;
            string zipPath = Path.ChangeExtension(fname2, ".zip");

            using (FileStream zipFile = new FileStream(zipPath, FileMode.Create)){
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create)){
                    foreach (DItem item in selectedItems)
                    {
                        string fileName = Path.GetFileName(item.Path);
                        ZipArchiveEntry zipArchive = archive.CreateEntryFromFile(item.Path, GetUniqueFileName(fileName));
                    }
                }

                ItemsUpdater.Update(context);
            }
        }

        private void UnZip_Click(object sender, RoutedEventArgs e)
        {
            List<DItem> selectedItems = new List<DItem>(ItemsListBox.SelectedItems.Cast<DItem>());

            foreach (DItem item in selectedItems)
            {
                string zipPath = Path.ChangeExtension(item.Path, ".zip");

                using (FileStream zipFile = new FileStream(zipPath, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
                    {
                        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileNameWithoutExtension(item.Path));

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string entryFullName = Path.Combine(directoryPath, entry.FullName);

                            Directory.CreateDirectory(Path.GetDirectoryName(entryFullName)!);
                            entry.ExtractToFile(entryFullName, overwrite: true);
                        }
                    }
                }
            }

            ItemsUpdater.Update(context);
        }

        private void UnZipTo_Click(object sender, RoutedEventArgs e)
        {
            List<DItem> selectedItems = new List<DItem>(ItemsListBox.SelectedItems.Cast<DItem>());

            foreach (DItem selectedItem in selectedItems)
            {
                string zipPath = Path.ChangeExtension(selectedItem.Path, ".zip");

                using (FileStream zipFile = new FileStream(zipPath, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Read))
                    {
                        OpenFolderDialog openFolderDialog = new OpenFolderDialog();
                        if (openFolderDialog.ShowDialog() == true)
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string destinationPath = Path.Combine(openFolderDialog.FolderName, entry.FullName);
                                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                                entry.ExtractToFile(destinationPath, overwrite: true);
                            }
                            MessageBox.Show($"Успішно розархівовано! {openFolderDialog.FolderName}", "Розархівовано", MessageBoxButton.OK); // Додати картинку в форматі ПЕЕСДЕ
                        }
                    }
                }
            }

            ItemsUpdater.Update(context);
        }
    }
}