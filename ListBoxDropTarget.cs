using System.Windows.Controls;

public class ListBoxDropTarget : ListBox, IAcceptDrop
{
    public bool OnDrop(object data)
    {
        this.Items.Add(data);
        return true;
    }
}

