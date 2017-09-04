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
        public string[] Styles { get; set; }
        public List<Style> _Styles { get; set; }
        public string[] Genres { get; set; }
    }

    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Label
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CatNo { get; set; }
    }

    public class Style
    {
        public string Name { get; set; }
    }
}
