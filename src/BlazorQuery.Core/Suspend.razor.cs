using Microsoft.AspNetCore.Components;

namespace BlazorQuery.Core;

public partial class Suspend<TIn, TOut> : ComponentBase
    where TIn : IEquatable<TIn>
{
    [Parameter, EditorRequired]
    public required IQuery<TIn, TOut> Foo { get; init; }

    [Parameter, EditorRequired]
    public required TIn Args { get; init; }

    [Parameter, EditorRequired]
    public required RenderFragment Body { get; init; }

    [Parameter]
    public bool AutoLoading { get; set; } = true;

    [Parameter]
    public RenderFragment? Loading { get; set; }

    [Parameter]
    public RenderFragment? Error { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Foo.Args = Args;

        if (AutoLoading)
        {
            await Foo.TriggerAsync();
        }
    }
}
