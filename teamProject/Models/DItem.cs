using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace teamProject.Models
{
    [AddINotifyPropertyChangedInterface]
    public class DItem
    {
        private List<string> Units = new List<string>()
        {
            "Байт", "КБ", "МБ", "ГБ", "ТБ", "ПТ"
        };

        private const int MAX_NAME_SIZE = 25;

        public string Name { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public long Size { get; set; }
        public string SizeString { get; set; } = "Розрахунок...";
        public string IconPath { get; set; }
        public Visibility ProgressVisibility { get; set; }
        public int ProgressSize { get; set; }
        public double PercentSize { get; set; }
        public long UsedSpace { get; set; }

        public DItem()
        {
            Name = null!;
            Date = null!;
            Path = null!;
            IconPath = null!;
            ProgressVisibility = Visibility.Collapsed;
            ProgressSize = 155;
        }

        public DItem(string name, DateTime date, string path, string iconPath)
        {
            if (name.Length >= MAX_NAME_SIZE)
            {
                Name = $"{name.Substring(0, MAX_NAME_SIZE - 1)}...";
            }
            else
            {
                Name = name;
            }

            Date = DateUK.ConvertDate(date);
            Path = path;
            IconPath = iconPath;
            ProgressVisibility = Visibility.Collapsed;
            ProgressSize = 155;
        }

        public void UpdateItemSize(long size)
        {
            Size = size;
            SizeString = UpdateSize(size);
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

    [AddINotifyPropertyChangedInterface]
    public class DDrive : DItem
    {
        public long TotalSpace { get; set; }
        public string TotalSpaceString { get; set; } = "";
        public long FreeSpace { get; set; }
        public string FreeSpaceString { get; set; } = "";

        public DDrive(DriveInfo driveInfo)
        {
            Path = driveInfo.Name;
            TotalSpace = driveInfo.TotalSize;
            FreeSpace = driveInfo.AvailableFreeSpace;
            UsedSpace = TotalSpace - FreeSpace;
            PercentSize = ((double)UsedSpace / TotalSpace) * 100;
            ProgressVisibility = Visibility.Visible;
            SizeString = "";
            ProgressSize = 115;
            TotalSpaceString = UpdateSize(TotalSpace);
            FreeSpaceString = UpdateSize(FreeSpace);
            Date = $"{FreeSpaceString} вільно з {TotalSpaceString}";

            InitializeDiskVisualComponents(driveInfo);
        }

        public void InitializeDiskVisualComponents(DriveInfo driveInfo)
        {
            if (driveInfo.DriveType == DriveType.Fixed)
            {
                Name = $"Локальний диск ({driveInfo.Name.Substring(0, driveInfo.Name.Length - 1)})";
                IconPath = "Assets/hdd.png";
            }
            //==================================================================================================================================================
            else if (driveInfo.DriveType == DriveType.Removable)
            {
                Name = $"USB пристрій ({driveInfo.Name.Substring(0, driveInfo.Name.Length - 1)})";
                IconPath = "Assets/hdd.png"; // ЮРА ЗРОБИ ІКОНКУ ЮСБ ПРИСТРОЇВ І ЗБЕРЕЖИ В ПЕEСДE ФОРМАТІ :D
            }
            else if (driveInfo.DriveType == DriveType.CDRom)
            {
                Name = $"DVD привід ({driveInfo.Name.Substring(0, driveInfo.Name.Length - 1)})";
                IconPath = "Assets/hdd.png"; // ЮРА ЗРОБИ ІКОНКУ ДВД ПРИСТРОЇВ І ЗБЕРЕЖИ В ПЕEСДE ФОРМАТІ :D
            }
            else if (driveInfo.DriveType == DriveType.Network)
            {
                Name = $"Мережевий диск ({driveInfo.Name.Substring(0, driveInfo.Name.Length - 1)})";
                IconPath = "Assets/hdd.png"; // ЮРА ЗРОБИ ІКОНКУ МЕРЕЖЕВИХ ПРИСТРОЇВ І ЗБЕРЕЖИ В ПЕEСДE ФОРМАТІ :D
            }
            else
            {
                Name = $"Невідомий пристрій ({driveInfo.Name.Substring(0, driveInfo.Name.Length - 1)})";
                IconPath = "Assets/hdd.png"; // ЮРА ЗРОБИ ІКОНКУ НЕВІДОМОГО ПРИСТРОЮ І ЗБЕРЕЖИ В ПЕEСДE ФОРМАТІ :D
            }
            //==================================================================================================================================================
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class DDirectory : DItem
    {
        public DDirectory(string name, DateTime date, string path, string iconPath) : base(name, date, path, iconPath) { }

        public DDirectory(string name, string path, string iconPath)
        {
            Name = name;
            Path = path;
            IconPath = iconPath;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class DFile : DItem
    {
        public DFile(string name, DateTime date, string path, long size, string iconPath) : base(name, date, path, iconPath)
        {
            Path = path;
            UpdateItemSize(size);
            IconPath = iconPath;
        }
    }
}
