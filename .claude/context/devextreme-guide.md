
# DevExtreme Integration Guide for ASP.NET Core

This guide provides step-by-step instructions for integrating DevExtreme components into an ASP.NET Core MVC project.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [NuGet Package Installation](#nuget-package-installation)
3. [Project Configuration](#project-configuration)
4. [Layout Setup](#layout-setup)
5. [Common Implementation Patterns](#common-implementation-patterns)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

## Prerequisites

- ASP.NET Core 8.0 or later
- Visual Studio 2022 or VS Code
- Basic knowledge of ASP.NET Core MVC

## NuGet Package Installation

### Configure DevExpress NuGet Feed

First, you need to configure the DevExpress NuGet feed to access the packages. Create or update a 'nuget.config' file in your solution root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="DevExpress" value="https://nuget.devexpress.com/3y9KIRRtr3OFOrqZMfgTJsh9l11xVgamEZxlND3doTwNvkvCBG/api/v3/index.json" />
  </packageSources>
</configuration>
```
### Add Package References

Add the following packages to your Web project's .csproj file:

```xml
<PackageReference Include="DevExtreme.AspNet.Core" Version="24.2.7" />
<PackageReference Include="DevExtreme.AspNet.Data" Version="5.0.1" />
```

### Restore Packages

After configuring the NuGet feed, restore the packages:

```bash
dotnet restore
```
## DevExpress License
If devexpress licence key is required, it is:
- License: ewogICJmb3JtYXQiOiAxLAogICJjdXN0b21lcklkIjogIjc4NDM2ODFmLWI3NzEtNDI4Ny05NjE1LWQwOTRkYmJiMDYyOSIsCiAgIm1heFZlcnNpb25BbGxvd2VkIjogMjQyCn0=.CCH+G4eIkM/TRvDurM01cW6CcQqvVqo55BDLKy9dFDBeCtKp48qjZusJLR+iXqIEaDOXZq6Y2OfpAqWodF3Hp0ZfbKClbb1HEN3+TMuFjxFx2lSNi+vp8JZDKvcMd2vYAplXHw==

## Project Configuration

### 1. Program.cs Configuration

Add JSON serialization options to preserve property names (DevExtreme expects PascalCase):

```csharp
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
```

### 2. _ViewImports.cshtml Setup

Add DevExtreme namespace to both Pages and Views _ViewImports files:

```razor
@using DevExtreme.AspNet.Mvc
@addTagHelper *, DevExtreme.AspNet.Mvc
```

### 3. _Layout.cshtml Configuration

Add the following to your layout file:

#### In the <head> section:
```html
<!-- DevExtreme Themes -->
<link rel="stylesheet" href="https://cdn3.devexpress.com/jslib/24.2.7/css/dx.light.css" />
<link rel="stylesheet" href="https://cdn3.devexpress.com/jslib/24.2.7/css/dx-office-white.css" />
```

#### Before closing </body> tag (after jQuery):
```html
<!-- jQuery (required by DevExtreme) -->
<script src="~/lib/jquery/dist/jquery.min.js"></script>

<!-- DevExtreme JS -->
<script src="https://cdn3.devexpress.com/jslib/24.2.7/js/dx.all.js"></script>
<script src="https://cdn3.devexpress.com/jslib/24.2.7/js/dx.aspnet.mvc.js"></script>

<!-- Optional: Set theme and localization -->
<script>
    DevExpress.ui.themes.current("office.white");
    DevExpress.localization.locale("en-GB");
</script>
```

## Common Implementation Patterns

### 1. DataGrid with Server-Side Operations

#### Controller Action for Data Loading:
```csharp
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;

[HttpGet]
public async Task<IActionResult> GetData(DataSourceLoadOptions loadOptions)
{
    var data = await _service.GetAllAsync();
    return Json(DataSourceLoader.Load(data, loadOptions));
}
```

#### View Implementation:
```razor
@(Html.DevExtreme().DataGrid<YourModel>()
    .ID("dataGrid")
    .DataSource(d => d.Mvc()
        .Controller("YourController")
        .LoadAction("GetData")
        .Key("Id"))
    .RemoteOperations(true)
    .Columns(columns =>
    {
        columns.AddFor(m => m.Property1);
        columns.AddFor(m => m.Property2)
            .Caption("Custom Caption")
            .Width(150);
        columns.Add()
            .Type(GridCommandColumnType.Buttons)
            .Buttons(b =>
            {
                b.Add().Text("Edit").Icon("edit")
                    .OnClick("function(e) { editItem(e.row.data); }");
            });
    })
    .FilterRow(f => f.Visible(true))
    .Paging(p => p.PageSize(20))
    .Selection(s => s.Mode(SelectionMode.Single))
)
```

### 2. Form with Validation

```razor
@(Html.DevExtreme().Form<YourViewModel>()
    .ID("form")
    .FormData(Model)
    .ColCount(2)
    .LabelLocation(FormLabelLocation.Top)
    .Items(items =>
    {
        items.AddSimpleFor(m => m.Name)
            .Label(l => l.Text("Name *"))
            .Editor(e => e.TextBox()
                .Placeholder("Enter name")
                .ShowClearButton(true))
            .IsRequired(true);
        
        items.AddSimpleFor(m => m.Date)
            .Label(l => l.Text("Date *"))
            .Editor(e => e.DateBox()
                .DisplayFormat("dd/MM/yyyy")
                .Type(DateBoxType.Date))
            .IsRequired(true);
        
        items.AddSimpleFor(m => m.CategoryId)
            .Label(l => l.Text("Category"))
            .Editor(e => e.SelectBox()
                .DataSource(d => d.Mvc()
                    .Controller("Categories")
                    .LoadAction("GetCategories"))
                .DisplayExpr("Name")
                .ValueExpr("Id")
                .Placeholder("Select category"));
    })
    .ValidationGroup("formValidation")
)

<script>
    function submitForm() {
        var formInstance = #form.dxForm("instance");
        var result = formInstance.validate();
        
        if (result.isValid) {
            // Submit form
           $("form").submit();
        }
    }
</script>
```

### 3. SelectBox/TagBox with Remote Data

```razor
@(Html.DevExtreme().SelectBox()
    .ID("categorySelect")
    .DataSource(d => d.Mvc()
        .Controller("Categories")
        .LoadAction("GetCategories")
        .Key("Id"))
    .DisplayExpr("Name")
    .ValueExpr("Id")
    .Value(Model.CategoryId)
    .Placeholder("Select a category")
    .ShowClearButton(true)
    .SearchEnabled(true)
    .OnValueChanged("onCategoryChanged")
)
```

### 4. Custom Cell Templates

```razor
columns.AddFor(m => m.Status)
    .CellTemplate(@<text>
        <% if (data.Status === 'Active') { %>
            <span class="badge bg-success">Active</span>
        <% } else { %>
            <span class="badge bg-secondary">Inactive</span>
        <% } %>
    </text>);
```

## Best Practices

### 1. Data Loading
- Always use DataSourceLoader.Load() for server-side operations
- Return data in the format DevExtreme expects
- Use DataSourceLoadOptions for filtering, sorting, and paging

### 2. Naming Conventions
- Keep property names in PascalCase
- Use meaningful IDs for components
- Follow consistent naming patterns

### 3. Performance
- Enable RemoteOperations(true) for large datasets
- Use server-side filtering and paging
- Implement proper indexing on database

### 4. Validation
- Use DevExtreme's built-in validation
- Combine with server-side validation
- Provide clear error messages

### 5. Styling
- Choose a consistent theme
- Use DevExtreme's CSS classes
- Avoid inline styles

## Troubleshooting

### Common Issues and Solutions

#### 1. DataGrid not loading data
**Problem**: Grid shows "No data"
**Solution**: 
- Check controller action returns correct format
- Verify property names match (case-sensitive)
- Check browser console for errors

#### 2. SelectBox/TagBox not showing items
**Problem**: Dropdown is empty
**Solution**:
- Ensure DisplayExpr and ValueExpr match data properties
- Check data source URL is correct
- Verify controller returns data

#### 3. Form validation not working
**Problem**: Form submits without validation
**Solution**:
- Add ValidationGroup to form
- Call validate() before submission
- Check IsRequired is set on fields

#### 4. Theme not applying
**Problem**: Components look unstyled
**Solution**:
- Ensure CSS files load before JS
- Check theme name is correct
- Clear browser cache

#### 5. JavaScript errors
**Problem**: "DevExpress is not defined"
**Solution**:
- Load jQuery before DevExtreme
- Check CDN URLs are accessible
- Ensure scripts load in correct order

## Example: Complete CRUD Implementation

### Model
```csharp
public class ItemViewModel
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; }
}
```

### Controller
```csharp
public class ItemsController : Controller
{
    private readonly IItemService _service;
    
    public ItemsController(IItemService service)
    {
        _service = service;
    }
    
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetItems(DataSourceLoadOptions loadOptions)
    {
        var items = await _service.GetAllAsync();
        return Json(DataSourceLoader.Load(items, loadOptions));
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateItem(string values)
    {
        var model = new ItemViewModel();
        JsonConvert.PopulateObject(values, model);
        
        if (!TryValidateModel(model))
            return BadRequest(ModelState);
        
        await _service.CreateAsync(model);
        return Ok();
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateItem(int key, string values)
    {
        var item = await _service.GetByIdAsync(key);
        JsonConvert.PopulateObject(values, item);
        
        if (!TryValidateModel(item))
            return BadRequest(ModelState);
        
        await _service.UpdateAsync(item);
        return Ok();
    }
    
    [HttpDelete]
    public async Task<IActionResult> DeleteItem(int key)
    {
        await _service.DeleteAsync(key);
        return Ok();
    }
}
```

### View
```razor
@{
    ViewData["Title"] = "Items";
}

<h1>Items Management</h1>

@(Html.DevExtreme().DataGrid<ItemViewModel>()
    .ID("itemsGrid")
    .DataSource(d => d.Mvc()
        .Controller("Items")
        .LoadAction("GetItems")
        .InsertAction("CreateItem")
        .UpdateAction("UpdateItem")
        .DeleteAction("DeleteItem")
        .Key("Id"))
    .RemoteOperations(true)
    .Columns(columns =>
    {
        columns.AddFor(m => m.Name);
        columns.AddFor(m => m.Date)
            .DataType(GridColumnDataType.Date);
        columns.AddFor(m => m.CategoryId)
            .Lookup(lookup => lookup
                .DataSource(d => d.Mvc()
                    .Controller("Categories")
                    .LoadAction("GetCategories"))
                .DisplayExpr("Name")
                .ValueExpr("Id"));
        columns.AddFor(m => m.IsActive)
            .DataType(GridColumnDataType.Boolean);
    })
    .Editing(e => e
        .Mode(GridEditMode.Popup)
        .AllowAdding(true)
        .AllowUpdating(true)
        .AllowDeleting(true)
        .Popup(p => p
            .Title("Item Details")
            .ShowTitle(true)
            .Width(700)
            .Height(450)))
    .FilterRow(f => f.Visible(true))
    .Paging(p => p.PageSize(20))
)
```

## Additional Resources

- [DevExtreme Documentation](https://js.devexpress.com/Documentation/)
- [DevExtreme ASP.NET Core Demos](https://demos.devexpress.com/ASPNetCore/)
- [DevExtreme Support Center](https://supportcenter.devexpress.com/)

## Version Compatibility

| DevExtreme Version | ASP.NET Core Version | Notes |
|-------------------|---------------------|-------|
| 24.2.x | 6.0, 7.0, 8.0, 9.0 | Latest stable |
| 24.1.x | 6.0, 7.0, 8.0 | Previous version |
| 23.2.x | 6.0, 7.0 | Legacy support |

Always check the [official documentation](https://docs.devexpress.com/AspNetCore/401016/devextreme-aspnet-mvc-controls) for the most up-to-date compatibility information.

```
