using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace AssetRipper.GUI.Electron.Pages.Settings.DropDown;

public readonly record struct DropDownParameters(EditModel ParentModel, IHtmlHelper<EditModel> ParentHtml, DropDownSetting Setting, Expression<Func<EditModel, string?>> Expression);
