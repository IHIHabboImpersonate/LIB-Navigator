using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IHI.Server.Libraries.Cecer1.Navigator;

public delegate void ListingEventHandler(object source, ListingEventArgs e);

public class ListingEventArgs : EventArgs
{
    public Navigator Navigator
    {
        get;
        set;
    }
}
public delegate void CategoryEventHandler(object source, CategoryEventArgs e);

public class CategoryEventArgs : ListingEventArgs
{
}