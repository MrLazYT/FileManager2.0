using System.Diagnostics;
using System.IO;
using System.Windows;

namespace teamProject.Models
{
    public class ItemSorter
    {
        public static async void SortFromAtoZ(UpdateContext context)
        {
            List<string> sortedItems = new List<string>();
            await Task.Run(() =>
            {
                try
                {
                    sortedItems.AddRange(Directory
                .EnumerateFileSystemEntries(context.Model.Path, "*.*")
                .OrderBy(path => path));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Неможливо доступитися до {context.Model.Path}: {ex.Message}");
                }
            });
            await ItemsByTypeUpdater.Update(sortedItems.ToArray(), context);
        }
        public static async void SortFromZtoA(UpdateContext context)
        {
            List<string> sortedItems = new List<string>();
            await Task.Run(() =>
            {
                try
                {
                    sortedItems.AddRange(Directory
                .EnumerateFileSystemEntries(context.Model.Path, "*.*")
                .OrderByDescending(path => path));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Неможливо доступитися до {context.Model.Path}: {ex.Message}");
                }
            });
            await ItemsByTypeUpdater.Update(sortedItems.ToArray(), context);
        }

        public static async void SortDate(UpdateContext context)
        {
            List<DItem> sortedItems = new List<DItem>();
            await Task.Run(() =>
            {
                try
                {
                    var items = Directory.EnumerateFileSystemEntries(context.Model.Path, "*.*")
                        .Select(path =>
                        {
                            var itemName = System.IO.Path.GetFileName(path);
                            var itemDate = Directory.GetLastWriteTime(path);

                            if (Directory.Exists(path))
                            {
                                return new DDirectory(itemName, itemDate, path, "Assets/folder.png") as DItem;
                            }
                            else
                            {
                                var itemSize = new FileInfo(path).Length;
                                string iconPath = ItemIcon.Get(itemName);

                                return new DFile(itemName, itemDate, path, itemSize, iconPath) as DItem;
                            }
                        })
                        .OrderBy(item => item.Date);

                    sortedItems.AddRange(items);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        context.Model.ClearItems();

                        foreach (var item in sortedItems)
                        {
                            context.Model.AddItem(item);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Неможливо доступитися до {context.Model.Path}: {ex.Message}");
                }
            });


        }
        public static async void SortDateDesc(UpdateContext context)
        {
            List<DItem> sortedItems = new List<DItem>();
            await Task.Run(() =>
            {
                try
                {
                    var items = Directory.EnumerateFileSystemEntries(context.Model.Path, "*.*")
                        .Select(path =>
                        {
                            var itemName = System.IO.Path.GetFileName(path);
                            var itemDate = Directory.GetLastWriteTime(path);

                            if (Directory.Exists(path))
                            {
                                return new DDirectory(itemName, itemDate, path, "Assets/folder.png") as DItem;
                            }
                            else
                            {
                                var itemSize = new FileInfo(path).Length;
                                string iconPath = ItemIcon.Get(itemName);

                                return new DFile(itemName, itemDate, path, itemSize, iconPath) as DItem;
                            }
                        })
                        .OrderByDescending(item => item.Date);

                    sortedItems.AddRange(items);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        context.Model.ClearItems();

                        foreach (var item in sortedItems)
                        {
                            context.Model.AddItem(item);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Неможливо доступитися до {context.Model.Path}: {ex.Message}");
                }
            });

        }
        public static async void SortSize(UpdateContext context)
        {
            List<DItem> sortedItems = new List<DItem>();
            await Task.Run(() =>
            {
                try
                {
                    var items = Directory.EnumerateFileSystemEntries(context.Model.Path, "*.*")
                        .Select(path =>
                        {
                            var itemName = System.IO.Path.GetFileName(path);
                            var itemDate = Directory.GetLastWriteTime(path);

                            if (Directory.Exists(path))
                            {
                                return new DDirectory(itemName, itemDate, path, "Assets/folder.png") as DItem;
                            }
                            else
                            {
                                var itemSize = new FileInfo(path).Length;
                                string iconPath = ItemIcon.Get(itemName);

                                return new DFile(itemName, itemDate, path, itemSize, iconPath) as DItem;
                            }
                        })
                        .OrderBy(item => item.Size);

                    sortedItems.AddRange(items);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        context.Model.ClearItems();

                        foreach (var item in sortedItems)
                        {
                            context.Model.AddItem(item);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Неможливо доступитися до {context.Model.Path}: {ex.Message}");
                }
            });


        }
        public static async void SortSizeDesc(UpdateContext context)
        {
            List<DItem> sortedItems = new List<DItem>();
            await Task.Run(() =>
            {
                try
                {
                    var items = Directory.EnumerateFileSystemEntries(context.Model.Path, "*.*")
                        .Select(path =>
                        {
                            var itemName = System.IO.Path.GetFileName(path);
                            var itemDate = Directory.GetLastWriteTime(path);

                            if (Directory.Exists(path))
                            {
                                return new DDirectory(itemName, itemDate, path, "Assets/folder.png") as DItem;
                            }
                            else
                            {
                                var itemSize = new FileInfo(path).Length;
                                string iconPath = ItemIcon.Get(itemName);

                                return new DFile(itemName, itemDate, path, itemSize, iconPath) as DItem;
                            }
                        })
                        .OrderByDescending(item => item.Size);

                    sortedItems.AddRange(items);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        context.Model.ClearItems();

                        foreach (var item in sortedItems)
                        {
                            context.Model.AddItem(item);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Неможливо доступитися до {context.Model.Path}: {ex.Message}");
                }
            });
        }
    }
}