using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace teamProject.Models
{
    public class UpdateContext
    {
        public XamlModel Model { get; set; }
        public ListBox ItemsListBox { get; set; }
        public ListBox FolderListBox { get; set; }
        public List<CancelTaskToken> Tokens { get; set; }
        public Button BackBtn { get; set; }
        public Button NextBtn { get; set; }

        public UpdateContext(XamlModel model, ListBox itemsListBox, ListBox folderListBox, List<CancelTaskToken> tokens, Button backBtn, Button nextBtn)
        {
            Model = model;
            ItemsListBox = itemsListBox;
            FolderListBox = folderListBox;
            Tokens = tokens;
            BackBtn = backBtn;
            NextBtn = nextBtn;
        }
    }
}
