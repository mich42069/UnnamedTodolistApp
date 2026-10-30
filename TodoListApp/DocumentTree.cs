namespace TodoListApp;

public static class DocumentTree
{
    public static Android.Net.Uri? FindChild(ContentResolver resolver, Android.Net.Uri treeUri, string name)
    {
        var children = DocumentsContract.BuildChildDocumentsUriUsingTree(
            treeUri, DocumentsContract.GetTreeDocumentId(treeUri));
        using var cursor = resolver.Query(children!,
            [DocumentsContract.Document.ColumnDocumentId, DocumentsContract.Document.ColumnDisplayName],
            null, null, null);

        while (cursor is not null && cursor.MoveToNext())
            if (cursor.GetString(1) == name)
                return DocumentsContract.BuildDocumentUriUsingTree(treeUri, cursor.GetString(0));
        return null;
    }
}