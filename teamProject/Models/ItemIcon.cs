using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teamProject.Models
{
    public class ItemIcon
    {
        public static string Get(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName).ToLower();
            string image = "";

            if (extension == ".txt")
            {
                image = "Assets/document.png";
            }
            else if (extension == ".json" || extension == ".xml" || extension == ".xaml" || extension == ".cs" || extension == ".cpp" || extension == ".html" || extension == ".css")
            {
                image = "Assets/dev-file.png";
            }
            else if (extension == ".ini" || extension == ".dll")
            {
                image = "Assets/sett-file.png";
            }
            else if (extension == ".exe")
            {
                image = "Assets/exe-file.png";
            }
            else if (extension == ".js")
            {
                image = "Assets/js-file.png";
            }
            else if (extension == ".java")
            {
                image = "Assets/java.png";
            }
            else if (extension == ".png" || extension == ".jpg" || extension == ".jpeg")
            {
                image = "Assets/picture.png";
            }
            else if (extension == ".mp3" || extension == ".wav" || extension == ".ogg")
            {
                image = "Assets/music.png";
            }
            else if (extension == ".mp4" || extension == ".mkv" || extension == ".mpeg" || extension == ".avi")
            {
                image = "Assets/video.png";
            }
            else if (extension == ".zip" || extension == ".rar" || extension == ".7z")
            {
                image = "Assets/archive.png";
            }
            else if (extension == ".py")
            {
                image = "Assets/python.png";
            }
            else if (extension == ".doc" || extension == ".docx" || extension == ".docm" || extension == ".ppt" || extension == ".pptx" || extension == ".xls" || extension == ".xlsm" || extension == ".xlsx" || extension == ".accdb")
            {
                image = "Assets/mc-office.png";
            }
            else if (extension == ".psd" || extension == ".ai" || extension == ".indd" || extension == ".prproj")
            {
                image = "Assets/adobe.png";
            }
            else
            {
                image = "Assets/file.png";
            }

            return image;
        }
    }
}
