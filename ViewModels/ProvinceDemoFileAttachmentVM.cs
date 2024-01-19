using Microsoft.AspNetCore.Http;
using SMSS.Models;
using System.Collections.Generic;

namespace SMSS.ViewModels
{
    public class ProvinceDemoFileAttachmentVM
    {
        public int Id { get; set; }
        public string ProvinceName { get; set; }

        public string ContentType { get; set; }
        public string FileName { get; set; }

        public IFormFile Attachment { get; set; }

        public List<ProvinceDemoFileAttachment> ProvinceDemoFileAttachments { get; set; }

    }
}
