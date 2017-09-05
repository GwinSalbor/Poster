using System.Collections.Generic;

namespace Poster
{
    public class DiscogsRelease
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string Country { get; set; }
        public List<Artist> Artists { get; set; }
        public List<Label> Labels { get; set; }
        public List<string> Styles { get; set; }
        public string[] Genres { get; set; }
    }
}
