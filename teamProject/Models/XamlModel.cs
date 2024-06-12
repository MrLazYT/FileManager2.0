using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teamProject.Models
{
    [AddINotifyPropertyChangedInterface]
    public class XamlModel
    {
        private ObservableCollection<DItem> items;
        private ObservableCollection<DDirectory> myFolders;
        private Stack<string> backPathHistory = new Stack<string>();
        public Stack<string> forwardPathHistory = new Stack<string>();
        public string Path { get; set; }
        public int ItemCount { get; set; } = 0;
        public long TotalSize { get; set; } = 0;
        public string TotalSizeString { get; set; } = "0 МБ";
        public IEnumerable<DItem> Items => items;
        public IEnumerable<DDirectory> MyFolders => myFolders;

        private List<string> Units = new List<string>()
            {
                "Байт", "КБ", "МБ", "ГБ", "ТБ", "ПТ"
            };

        public XamlModel()
        {
            Path = null!;

            items = new ObservableCollection<DItem>();
            myFolders = new ObservableCollection<DDirectory>()
            {
                new DDirectory("Диски", "<=Discs=>", "Assets/hdd.png"),
                new DDirectory("Робочий стіл", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Assets/computer.png"),
                new DDirectory("Завантаження", @$"C:\Users\{Environment.UserName}\Downloads", "Assets/download.png"),
                new DDirectory("Документи", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Assets/document.png"),
                new DDirectory("Зображення", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Assets/picture.png"),
                new DDirectory("Відео", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Assets/video.png"),
                new DDirectory("Музика", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "Assets/music.png"),
            };
        }

        public void RemoveForwardPath(string path)
        {
            if (forwardPathHistory.Contains(path))
            {
                var tempStack = new Stack<string>(forwardPathHistory.Count);

                while (forwardPathHistory.Count > 0)
                {
                    var currentPath = forwardPathHistory.Pop();

                    if (currentPath != path)
                    {
                        tempStack.Push(currentPath);
                    }
                }

                while (tempStack.Count > 0)
                {
                    forwardPathHistory.Push(tempStack.Pop());
                }
            }
        }

        public void AddItem(DItem item)
        {
            items.Add(item);
        }

        public void ClearItems()
        {
            items.Clear();
        }

        public void PushBackPath(string path)
        {
            backPathHistory.Push(path);
        }

        public string PopBackPath()
        {
            return backPathHistory.Count > 0 ? backPathHistory.Pop() : null!;
        }

        public void PushForwardPath(string path)
        {
            forwardPathHistory.Push(path);
        }

        public string PopForwardPath()
        {
            return forwardPathHistory.Count > 0 ? forwardPathHistory.Pop() : null!;
        }

        public bool CanGoBack()
        {
            return backPathHistory.Count > 0;
        }

        public bool CanGoForward()
        {
            return forwardPathHistory.Count > 0;
        }

        public void UpdateCount(int count)
        {
            ItemCount = count;
        }

        public void UpdateTotalSize(long size)
        {
            TotalSize = size;
            TotalSizeString = UpdateSize(size);
        }

        public string UpdateSize(long size)
        {
            string remainderString = "";
            long convertedSize = size;
            int unitIndex = ConvertUnit(ref convertedSize);
            long roundedSize = convertedSize;

            for (int i = 0; i < unitIndex; i++)
            {
                roundedSize *= 1024;
            }

            long byteDifference = (size - roundedSize);

            if (byteDifference > 0)
            {
                ConvertUnit(ref byteDifference);
                int remainder = (int)((100 / 1024.0) * byteDifference);

                remainderString = $",{remainder}";
            }

            return $"{convertedSize}{remainderString} {Units[unitIndex]}";
        }

        public int ConvertUnit(ref long unit)
        {
            int unitIndex = 0;

            while (unit >= 1024)
            {
                unit /= 1024;
                unitIndex++;
            }

            return unitIndex;
        }
    }
}
