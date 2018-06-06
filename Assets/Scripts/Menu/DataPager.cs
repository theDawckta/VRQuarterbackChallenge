using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class DataPager<T>
{
    protected abstract IEnumerable<T> DataToPage { get; }

    public int PageSize { get; set; }
    public virtual int PageIndex { get; protected set; }

    public int NumberOfPages
    {
        get
        {
            return (int)Math.Ceiling(DataToPage.Count() / (double)PageSize);
        }
    }

    public bool HasNextPage
    {
        get { return PageIndex < NumberOfPages - 1; }
    }

    public bool MoveToNextPage()
    {
        if (!HasNextPage)
            return false;

        PageIndex++;
        return true;
    }

    public bool HasPreviousPage
    {
        get { return PageIndex > 0; }
    }

    public bool MoveToPreviousPage()
    {
        if (!HasPreviousPage)
            return false;

        PageIndex--;
        return true;
    }

    public IEnumerable<T> GetCurrentPage()
    {
        return DataToPage.Skip(PageIndex * PageSize).Take(PageSize);
    }
}