using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace teamProject.Models
{
    public abstract class Updater
    {
        public static void Update(UpdateContext context) {}
    }

    public class ItemsUpdater : Updater
    {
        public static void Update(UpdateContext context)
        {
            if (context.Model.Path == "Диски")
            {
                DrivesUpdater.Update(context);
            }
            else
            {
                MyFoldersUpdater.Update(context);
                DirectoryUpdater.Update(context);
            }
        }
    }

    public class DrivesUpdater : Updater
    {
        public static void Update(UpdateContext context)
        {
            context.ItemsListBox.ItemsSource = new List<DItem>();
            context.Model.ClearItems();

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                DDrive dDrive = new DDrive(drive);

                context.Model.AddItem(dDrive);
            }
            context.ItemsListBox.ItemsSource = context.Model.Items;
            context.Model.Path = "Диски";

            context.Model.UpdateCount(context.ItemsListBox.Items.Count);
            TotalSizeUpdater.Update(context);

        }
    }

    public class MyFoldersUpdater : Updater
    {
        public static void Update(UpdateContext context)
        {
            bool isInMyFolderPath = false;

            foreach (DDirectory myFolder in context.Model.MyFolders)
            {
                if (context.Model.Path == myFolder.Path)
                {
                    isInMyFolderPath = true;
                    context.FolderListBox.SelectedItem = myFolder;

                    break;
                }
            }

            if (!isInMyFolderPath)
            {
                context.FolderListBox.SelectedItem = null;
            }
        }
    }

    public class DirectoryUpdater : Updater
    {
        public async static void Update(UpdateContext context)
        {
            context.Model.ClearItems();

            try
            {
                string[] directories = Directory.GetDirectories(context.Model.Path);
                string[] files = Directory.GetFiles(context.Model.Path);

                await ItemsByTypeUpdater.Update(directories, context);
                await ItemsByTypeUpdater.Update(files, context);

                context.ItemsListBox.ItemsSource = context.Model.Items;
                ItemsSizeUpdater.Update(context);
                context.Model.UpdateCount(context.ItemsListBox.Items.Count);
                TotalSizeUpdater.Update(context);
                ButtonStateUpdater.Update(context);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неможливо отримати доступ до папки {ex}", "Помилка відкриття папки", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ItemsByTypeUpdater : Updater
    {
        public static Task Update(string[] items, UpdateContext context)
        {
            return Task.Run(async () =>
            {
                foreach (string itemPath in items)
                {
                    await UpdateItem(itemPath, context);
                }
            });
        }

        private static Task UpdateItem(string itemPath, UpdateContext context)
        {
            return Task.Run(async () =>
            {
                string itemName = Path.GetFileName(itemPath);
                List<string> vanishedItems = new List<string>()
                { "$recycle.bin", "$windows.~ws", "$winreagent", "config.msi", "documents and settings",
                  "system volume information", "recovery", "msocache", "$av_asw", "boot", "application data",
                  "dumpstack.log", "dumpstack.log.tmp", "hiberfil.sys", "pagefile.sys", "swapfile.sys", "vfcompat.dll",
                  "bootmgr", "bootnxt", "boottel.dat", "autoexec.bat", "bootsect.bak", "config.sys", "io.sys",
                  "msdos.sys", "wfnei" };

                DateTime itemDate = Directory.GetLastWriteTime($"{itemPath}");
                DItem dItem = new DItem();

                if (Directory.Exists(itemPath) &&
                    !vanishedItems.Contains(itemName.ToLower()))
                {
                    dItem = await UpdateDirectory(itemPath);
                }
                else if (File.Exists(itemPath) &&
                         !vanishedItems.Contains(itemName.ToLower()))
                {
                    dItem = UpdateFile(itemPath);
                }

                AddItem(dItem, context);
            });
        }

        private static Task<DDirectory> UpdateDirectory(string dirPath)
        {
            return Task.Run(() =>
            {
                string dirName = Path.GetFileName(dirPath);
                DateTime dirDate = Directory.GetLastWriteTime($"{dirPath}");
                DDirectory dDirectory = new DDirectory(dirName, dirDate, dirPath, "Assets/folder.png");

                return dDirectory;
            });
        }

        private static DFile UpdateFile(string itemPath)
        {
            string itemName = Path.GetFileName(itemPath);
            DateTime itemDate = Directory.GetLastWriteTime($"{itemPath}");
            long itemSize = new FileInfo(itemPath).Length;
            string iconPath = ItemIcon.Get(itemName);

            return new DFile(itemName, itemDate, itemPath, itemSize, iconPath);
        }

        private static void AddItem(DItem dItem, UpdateContext context)
        {
            if (dItem.Name != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    context.Model.AddItem(dItem);
                });
            }
        }
    }

    public class TotalSizeUpdater : Updater
    {
        public static void Update(UpdateContext context)
        {
            long totalSize = 0;

            foreach (DItem dItem in context.Model.Items)
            {
                if (dItem is DFile)
                {
                    totalSize += dItem.Size;
                }
                else if (dItem is DDrive)
                {
                    DDrive dDrive = (DDrive)dItem;

                    totalSize += dDrive.TotalSpace - dDrive.FreeSpace;
                }
            }

            context.Model.UpdateTotalSize(totalSize);
        }
    }

    public class ItemsSizeUpdater : Updater
    {
        public static void Update(UpdateContext context)
        {
            try
            {
                foreach (DItem dItem in context.Model.Items)
                {
                    CancelTaskToken token = new CancelTaskToken();
                    context.Tokens.Add(token);

                    UpdateItemSize(dItem, token, context);
                }
            }
            catch (InvalidOperationException) { }
            catch (Exception ex)
            {
                MessageBox.Show($"Непередбачена помилка: {ex.Message}", "Помилка розрахунку розміру папки.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async void UpdateItemSize(DItem dItem, CancelTaskToken token, UpdateContext context)
        {
            if (dItem is DDirectory)
            {
                string itemPath = Path.Combine(context.Model.Path, dItem.Name);
                List<string> strings = new List<string>()
                {
                    "Users", "ProgramData", "All Users", "Default", "Windows"
                };

                if (strings.Contains(dItem.Name))
                {
                    dItem.SizeString = "Невідомо";
                }
                else
                {
                    long itemSize = await GetItemsSizeAsync(dItem, token, context);
                    dItem.UpdateItemSize(itemSize);
                }
            }
        }

        private static Task<long> GetItemsSizeAsync(DItem dItem, CancelTaskToken token, UpdateContext context)
        {
            return Task.Run(async () =>
            {
                long size = 0;

                while (!token.IsCancelRequested())
                {
                    try
                    {
                        size = GetItemsSizeFast(dItem);
                    }
                    catch
                    {
                        size = await GetItemsSizeLong(dItem.Path);
                    }

                    if (!token.IsCancelRequested())
                    {
                        token.Stop();
                    }
                }

                if (!token.IsCompleted())
                {
                    return 0;
                }
                else
                {
                    UpdateDirectorySize(size, context);

                    return size;
                }
            });
        }

        private static long GetItemsSizeFast(DItem dItem)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dItem.Path);
            long size = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);

            return size;
        }

        private static async Task<long> GetItemsSizeLong(string curDirectoryPath)
        {
            long size = 0;
            string[] directories = new string[] { };
            string[] files = new string[] { };

            try
            {
                directories = Directory.GetDirectories(curDirectoryPath);
                files = Directory.GetFiles(curDirectoryPath);
            }
            catch
            {
                return 0;
            }

            foreach (string directoryPath in directories)
            {
                size += await GetItemsSizeLong(directoryPath);
            }

            foreach (string filePath in files)
            {
                size += new FileInfo(filePath).Length;
            }

            return size;
        }

        private static void UpdateDirectorySize(long size, UpdateContext context)
        {
            context.Model.UpdateTotalSize(context.Model.TotalSize + size);
        }
    }

    public class ButtonStateUpdater : Updater
    {
        public static void Update(UpdateContext context)
        {
            DirectoryInfo parentDir = Directory.GetParent(context.Model.Path)!;

            context.BackBtn.IsEnabled = (parentDir != null);
            context.NextBtn.IsEnabled = context.Model.CanGoForward();
        }
    }
}