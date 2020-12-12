namespace BRD_Sport_Sem.Models
{
    public class Record
    {
        public string Name { get; set; }
        public string Author { get; set; }

        public Record() { }

        public Record(string name, string author)
        {
            Name = name;
            Author = author;
        }
    }
}