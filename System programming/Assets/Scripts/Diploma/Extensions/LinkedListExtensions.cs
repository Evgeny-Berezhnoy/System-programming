using System.Collections.Generic;

public static class LinkedListExtensions
{
    #region Extension methods

    public static LinkedListNode<T> LoopNext<T>(this LinkedListNode<T> linkedListNode)
    {
        if(linkedListNode.Next == null)
        {
            return linkedListNode.List.First;
        }
        else
        {
            return linkedListNode.Next;
        };
    }

    #endregion
}