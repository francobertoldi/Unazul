using SA.Config.Domain.Enums;
using Shared.Contract.Exceptions;

namespace SA.Config.Application.Commands.Workflows;

internal static class WorkflowValidationHelper
{
    public static void ValidateStateConfigs(WorkflowStateInput[] states)
    {
        for (var i = 0; i < states.Length; i++)
        {
            var state = states[i];
            if (!Enum.TryParse<FlowNodeType>(state.Type, ignoreCase: true, out var nodeType))
                throw new ValidationException("WORKFLOW_INVALID_NODE_TYPE", $"Tipo de nodo invalido: {state.Type}");

            var configs = state.Configs ?? [];

            switch (nodeType)
            {
                case FlowNodeType.ServiceCall:
                    RequireConfig(configs, "service_id", $"State '{state.Name}': service_call requires service_id");
                    RequireConfig(configs, "endpoint", $"State '{state.Name}': service_call requires endpoint");
                    RequireConfig(configs, "method", $"State '{state.Name}': service_call requires method");
                    break;

                case FlowNodeType.Decision:
                    RequireConfig(configs, "condition", $"State '{state.Name}': decision requires condition");
                    break;

                case FlowNodeType.SendMessage:
                    RequireConfig(configs, "channel", $"State '{state.Name}': send_message requires channel");
                    RequireConfig(configs, "template_id", $"State '{state.Name}': send_message requires template_id");
                    break;

                case FlowNodeType.Timer:
                    RequireConfig(configs, "timer_minutes", $"State '{state.Name}': timer requires timer_minutes");
                    var timerCfg = configs.FirstOrDefault(c => c.Key == "timer_minutes");
                    if (timerCfg != null && (!int.TryParse(timerCfg.Value, out var minutes) || minutes <= 0))
                        throw new ValidationException("WORKFLOW_INVALID_TIMER", $"Estado '{state.Name}': timer_minutes debe ser mayor a 0");
                    break;

                case FlowNodeType.DataCapture:
                    if (state.Fields is null || state.Fields.Length == 0)
                        throw new ValidationException("WORKFLOW_DATA_CAPTURE_FIELDS_REQUIRED", $"Estado '{state.Name}': data_capture requiere al menos 1 campo");
                    foreach (var field in state.Fields)
                    {
                        if (string.IsNullOrWhiteSpace(field.FieldName))
                            throw new ValidationException("WORKFLOW_FIELD_NAME_REQUIRED", $"Estado '{state.Name}': el nombre del campo es obligatorio");
                        if (string.IsNullOrWhiteSpace(field.FieldType))
                            throw new ValidationException("WORKFLOW_FIELD_TYPE_REQUIRED", $"Estado '{state.Name}': el tipo del campo es obligatorio");
                    }
                    break;
            }
        }
    }

    public static void ValidateTransitions(WorkflowTransitionInput[] transitions, int stateCount)
    {
        var seen = new HashSet<(int, int)>();

        foreach (var tr in transitions)
        {
            if (tr.FromStateIndex < 0 || tr.FromStateIndex >= stateCount)
                throw new ValidationException("WORKFLOW_INVALID_TRANSITION_INDEX", $"Indice de transicion invalido: FromStateIndex {tr.FromStateIndex}");

            if (tr.ToStateIndex < 0 || tr.ToStateIndex >= stateCount)
                throw new ValidationException("WORKFLOW_INVALID_TRANSITION_INDEX", $"Indice de transicion invalido: ToStateIndex {tr.ToStateIndex}");

            if (tr.FromStateIndex == tr.ToStateIndex)
                throw new ValidationException("WORKFLOW_SELF_REFERENCING_TRANSITION", "No se permite una transicion que apunte al mismo estado.");

            if (!seen.Add((tr.FromStateIndex, tr.ToStateIndex)))
                throw new ConflictException("WORKFLOW_DUPLICATE_TRANSITION", "Ya existe una transicion entre estos estados.");

            if (tr.SlaHours.HasValue && tr.SlaHours.Value <= 0)
                throw new ValidationException("WORKFLOW_SLA_HOURS_MUST_BE_POSITIVE", "Las horas de SLA deben ser un valor positivo.");
        }
    }

    private static void RequireConfig(StateConfigInput[] configs, string key, string errorMessage)
    {
        var cfg = configs.FirstOrDefault(c => c.Key == key);
        if (cfg is null || string.IsNullOrWhiteSpace(cfg.Value))
            throw new ValidationException("WORKFLOW_MISSING_CONFIG", errorMessage);
    }
}
