using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Private_Note.Models
{
    public class Files
    {
        //[MaxLength(100)]
        [DisplayName("File Name")]
        public string FileName { get; set; }
        public string UserName { get; set; }
        //MaxLength(100)]
        [DisplayName("File Type")]
        public string FileType { get; set; }
        //[MaxLength]
        public byte[] File { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
