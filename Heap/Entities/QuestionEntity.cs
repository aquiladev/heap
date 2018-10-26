using Microsoft.WindowsAzure.Storage.Table;

namespace Heap.Entities
{
    public class QuestionEntity : TableEntity
    {
        public string Payload { get; set; }
        public string Aylien { get; set; }
    }
}
