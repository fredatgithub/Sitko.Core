using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OneOf;
using Sitko.Core.App.Localization;

namespace Sitko.Core.Blazor.AntDesignComponents.Components
{
    using System;
    using App.Blazor.Forms;

    public class AntForm<TEntity> : BaseAntForm<TEntity, AntForm<TEntity>>
        where TEntity : class, new()
    {
    }

    public partial class BaseAntForm<TEntity, TForm> where TEntity : class, new()
        where TForm : BaseAntForm<TEntity, TForm>
    {
        protected Form<TEntity>? AntFormInstance { get; set; }

        [Inject] protected ILocalizationProvider<TForm> LocalizationProvider { get; set; } = null!;

        [Parameter] public RenderFragment<TForm> ChildContent { get; set; } = null!;

        [Parameter] public string Layout { get; set; } = FormLayout.Horizontal;

        [Parameter] public ColLayoutParam LabelCol { get; set; } = new();

        [Parameter] public AntLabelAlignType? LabelAlign { get; set; }

        [Parameter]
        public OneOf<string, int> LabelColSpan
        {
            get => LabelCol.Span;
            set => LabelCol.Span = value;
        }

        [Parameter]
        public OneOf<string, int> LabelColOffset
        {
            get => LabelCol.Offset;
            set => LabelCol.Offset = value;
        }

        [Parameter] public ColLayoutParam WrapperCol { get; set; } = new();

        [Parameter]
        public OneOf<string, int> WrapperColSpan
        {
            get => WrapperCol.Span;
            set => WrapperCol.Span = value;
        }

        [Parameter]
        public OneOf<string, int> WrapperColOffset
        {
            get => WrapperCol.Offset;
            set => WrapperCol.Offset = value;
        }

        [Parameter] public string? Size { get; set; }

        [Parameter] public string? Name { get; set; }

        [Parameter] public bool ValidateOnChange { get; set; } = true;

        [Inject] protected MessageService MessageService { get; set; } = null!;

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            OnSuccess ??= () => MessageService.Success(LocalizationProvider["Entity saved successfully"]);
            OnError ??= error => MessageService.Error(error);
            OnException ??= exception => MessageService.Error(exception.ToString());
        }

        protected override Task<(bool IsNew, TEntity Entity)> GetEntityAsync() => GetEntity!();

        protected Task OnFormErrorAsync(EditContext editContext) =>
            MessageService.Error(string.Join(". ", editContext.GetValidationMessages()));

        public void Save() => AntFormInstance?.Submit();

        [Parameter] public Func<TEntity, Task<FormSaveResult>>? Add { get; set; }

        [Parameter] public Func<TEntity, Task<FormSaveResult>>? Update { get; set; }
        [Parameter] public Func<Task<(bool IsNew, TEntity Entity)>>? GetEntity { get; set; }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            if (GetEntity is null)
            {
                throw new InvalidOperationException("GetEntity is not defined");
            }

            if (Add is null)
            {
                throw new InvalidOperationException("Add is not defined");
            }

            if (Update is null)
            {
                throw new InvalidOperationException("Update is not defined");
            }
        }

        protected override Task<FormSaveResult> AddAsync(TEntity entity) => Add!(entity);

        protected override Task<FormSaveResult> UpdateAsync(TEntity entity) => Update!(entity);
    }
}
