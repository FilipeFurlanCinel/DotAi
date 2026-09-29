using System.ComponentModel;
using Domain.LlmAggregate.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using Usecases.Common;
using Usecases.Llm.Specifications;

namespace Usecases.Llm.Commands;

public sealed class RemoveLlmModelCommand : AsyncCommand<RemoveLlmModelCommand.Settings>
{
    private readonly IRepository<LlmModel> _llmRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveLlmModelCommand(IRepository<LlmModel> llmRepository, IUnitOfWork unitOfWork)
    {
        _llmRepository = llmRepository;
        _unitOfWork = unitOfWork;
    }

    public sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "<NAME_OR_ID>")]
        [Description("Model name or Id to remove.")]
        public string NameOrId { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        LlmModel? model = null;
        if (Guid.TryParse(settings.NameOrId, out var id))
            model = await _llmRepository.GetByIdAsync(id);

        model ??= await _llmRepository.GetBySpecAsync(new LlmModelByNameSpecification(settings.NameOrId));

        if (model is null)
        {
            AnsiConsole.MarkupLine($"[red]Model '{settings.NameOrId}' not found.[/]");
            return 1;
        }

        await _llmRepository.DeleteAsync(model);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        AnsiConsole.MarkupLine($"[green]Removed[/] {model.Name}");
        return 0;
    }
}
