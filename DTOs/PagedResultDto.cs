using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NDLP_Project.DTOs
{
    public class PagedResultDto<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; }
        public PagedResultDto(List<T> items, int totalRecords, int pageNumber, int pageSize)
        {
            Items = items;
            TotalRecords = totalRecords;
            PageNumber = pageNumber;
            PageSize = pageSize;
            // Calculate total pages: e.g. 105 records / 20 per page = 5.25 -> rounded up to 6 pages
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        }
    }
}