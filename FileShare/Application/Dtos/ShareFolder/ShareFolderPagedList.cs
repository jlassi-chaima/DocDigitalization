using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.ShareFolder
{
    public class ShareFolderPagedList
    {
        public Guid Id { get; set; }
        public string FolderPath { get; set; }
        public string ShareName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
