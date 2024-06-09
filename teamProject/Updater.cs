using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace teamProject
{
    public abstract class Updater
    {
        public static void Update() {}
    }

    public class ItemUpdater : Updater
    {
        public static void Update()
        {

        }
    }

    public class DriveUpdater : Updater
    {
        public static void Update(Model model)
        {
            model.ClearItems();

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                DDrive dDrive = new DDrive(drive);

                model.AddItem(dDrive);
            }

            model.Path = "Диски";
            model.UpdateTotalItemsCount();
            model.UpdateTotalSize();
        }
    }

    public class MyFolderUpdater : Updater
    {
        public static void Update()
        {

        }
    }
}
