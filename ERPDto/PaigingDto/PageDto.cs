namespace ERPDto.PaigingDto
{
    public class PageDto
    {
        private int _pageSize = 10;
        private int _pageIndex = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value is < 1 ? 10 : value > 100 ? 100 : value;
        }

        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value < 1 ? 1 : value;
        }

        public string? SearchTerm { get; set; }
        public int CategoryId { get; set; }
    }
}
