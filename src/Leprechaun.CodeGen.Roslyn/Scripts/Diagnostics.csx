using System.Linq;
using System.Text;
Log.Debug($"Emitting diagnostic templates for {ConfigurationName}...");

if (Templates != null)
{
    Code.AppendLine("// -- Templates");

    foreach (var template in Templates)
    {
        Code.AppendLine($"// {template.FullTypeName} ({template.Path} {template.Id})");

        foreach (var field in template.OwnFields)
        {
            Code.AppendLine($"\t// {field.CodeName} ({field.Id})");
            Code.AppendLine($"\t\t// Type: {field.Type}");
            Code.AppendLine($"\t\t// Section: {field.Section}");
            Code.AppendLine($"\t\t// Sort Order: {field.SortOrder}");
            Code.AppendLine($"\t\t// Source: {field.Source}");
        }

        Code.AppendLine(string.Empty);
    }
}

public void RenderRecursive(ItemCodeGenerationMetadata item, int depth = 1)
{
    StringBuilder indenter = new StringBuilder();
    for (int i = 0; i < depth; i++) indenter.Append(" ");

    Code.AppendLine($"// {indenter}> {item.Name} ({item.Id})");
    RenderFields(item, depth);
    foreach (var child in item.Children)
    {
        RenderRecursive(child, depth + 1);
    }
}

public void RenderFields(ItemCodeGenerationMetadata item, int depth)
{
    StringBuilder indenter = new StringBuilder();
    for (int i = 0; i < depth; i++) indenter.Append(" ");

    foreach (var fieldValue in item.ItemInfo.FieldValues)
    {
        Code.AppendLine($"// {indenter}     {fieldValue.FieldName} ({fieldValue.FieldId}) = {fieldValue.RawValue}");
    }
}

if (Items != null)
{
    Code.AppendLine("// -- Items");

	foreach (var item in Items.Where(i => i.Parent == null))
    {
        Code.AppendLine();
        Code.AppendLine($"// {item.Path} ({item.Id})");
        RenderFields(item, 0);
        RenderRecursive(item);
    }

    Code.AppendLine(string.Empty);
}